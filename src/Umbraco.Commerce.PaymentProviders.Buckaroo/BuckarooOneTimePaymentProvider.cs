using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuckarooSdk.DataTypes;
using BuckarooSdk.DataTypes.RequestBases;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    [PaymentProvider("buckaroo-onetime-payment", "Buckaroo One Time Payment", "Buckaroo one time payment provider")]
    public class BuckarooOneTimePaymentProvider : BuckarooPaymentProviderBase<BuckarooOneTimePaymentProvider, BuckarooOneTimeSettings>
    {
        public BuckarooOneTimePaymentProvider(
            UmbracoCommerceContext ctx,
            ILogger<BuckarooOneTimePaymentProvider> logger)
            : base(ctx, logger)
        { }

        public override bool CanFetchPaymentStatus => true;
        public override bool CanCapturePayments => false;
        public override bool CanCancelPayments => true;
        public override bool CanRefundPayments => false;

        // Don't finalize at continue as we will finalize async via webhook
        public override bool FinalizeAtContinueUrl => false;

        /// <summary>
        /// TODO - Dinh: double check the metadata
        /// </summary>
        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]
        {
            new TransactionMetaDataDefinition("buckarooSessionId", "Buckaroo Session ID"),
            new TransactionMetaDataDefinition("buckarooCustomerId", "Buckaroo Customer ID"),
            new TransactionMetaDataDefinition("buckarooPaymentIntentId", "Buckaroo Payment Intent ID"),
            new TransactionMetaDataDefinition("buckarooSubscriptionId", "Buckaroo Subscription ID"),
            new TransactionMetaDataDefinition("buckarooChargeId", "Buckaroo Charge ID"),
            new TransactionMetaDataDefinition("buckarooCardCountry", "Buckaroo Card Country"),
        };

        public override async Task<PaymentFormResult> GenerateFormAsync(PaymentProviderContext<BuckarooOneTimeSettings> context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            OrderReadOnly order = context.Order;
            CurrencyReadOnly currency = Context.Services.CurrencyService.GetCurrency(order.CurrencyId);
            string currencyCode = currency.Code.ToUpperInvariant();
            var data = new TransactionBase
            {
                AmountDebit = order.TransactionAmount.Value,
                Currency = currencyCode,
                Description = order.OrderNumber,
                Invoice = order.OrderNumber,
                Order = order.OrderNumber,
                ReturnUrl = GetContinueUrl(context),
                ReturnUrlCancel = GetCancelUrl(context),
                ReturnUrlError = GetErrorUrl(context), // ToDo: the error url is different than the provided continue and cancel urls (webhook urls),
                                                       // we probably want this from the method's arguments as well // ask Matt
                ReturnUrlReject = GetCancelUrl(context),
                StartRecurrent = false,
                // TODO: Dinh - get client information from httpcontext
                //ClientIp = new IpAddress { Address = HttpContext.Current.Request.UserHostAddress, Type = InternetProtocolVersion.IPv4 }, // ToDo: should we have a look at IPv6 as well?
                //ClientUserAgent = HttpContext.Current.Request.UserAgent,
                ContinueOnIncomplete = ContinueOnIncomplete.RedirectToHTML,
                // TODO - Dinh: ask Matt about these two
                //PushUrl = callbackUrl,
                //PushUrlFailure = callbackUrl,
            };

            BuckarooSdk.DataTypes.Response.RequestResponse response = await BuckarooClientHelper
                .GetAuthenticatedRequest(context.Settings)
                .TransactionRequest()
                .SetBasicFields(data)

                // ToDo: for now it's fine to select a service at Buckaroo, but it might be nice to implement available services etc.
                .NoServiceSelected()
                .Pay()
                .ExecuteAsync()
                .ConfigureAwait(false);

            return new PaymentFormResult
            {
                Form = new PaymentForm(response.RequiredAction.RedirectURL),
            };
        }

        public override async Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<BuckarooOneTimeSettings> context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            //try
            //{

            //    var buckarooRequest = GetWebhookRequest(request);

            //    if (!buckarooRequest.IsSuccess)
            //    {
            //        return CallbackResult.Ok();
            //    }

            //    var transactionInfo = new TransactionInfo
            //    {
            //        TransactionId = buckarooRequest.PaymentId,
            //        PaymentStatus = PaymentStatus.Captured,
            //        AmountAuthorized = buckarooRequest.Amount
            //    };

            //    return CallbackResult.Ok(transactionInfo);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error(ex, "Buckaroo - ProcessCallback");
            //}

            //return CallbackResult.BadRequest();
        }

        /// <summary>
        /// This should work in theory, but it's not used by Vendr yet...
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<ApiResult> FetchPaymentStatusAsync(PaymentProviderContext<BuckarooOneTimeSettings> context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            try
            {
                BuckarooOneTimeSettings settings = context.Settings;
                OrderReadOnly order = context.Order;
                BuckarooSdk.DataTypes.Response.StatusRequest.TransactionStatus status = BuckarooClientHelper
                    .GetAuthenticatedRequest(context.Settings)
                    .TransactionStatusRequest()
                    .Status(order.TransactionInfo.TransactionId)
                    .GetSingleStatus();

                return Task.FromResult(new ApiResult
                {
                    TransactionInfo = new TransactionInfoUpdate
                    {
                        TransactionId = status.PaymentKey,
                        PaymentStatus = status.Status.Code.ToPaymentStatus(),
                    },
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Buckaroo - FetchPaymentStatus");
            }

            return Task.FromResult(ApiResult.Empty);
        }

        public override Task<ApiResult> CancelPaymentAsync(PaymentProviderContext<BuckarooOneTimeSettings> context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);
            try
            {
                OrderReadOnly order = context.Order;
                if (order.TransactionInfo.PaymentStatus == PaymentStatus.Authorized)
                {
                    BuckarooSdk.DataTypes.Response.RequestResponse response = BuckarooClientHelper.GetAuthenticatedRequest(context.Settings)
                        .CancelTransactionRequest()
                        .CancelMultiple(new CancelTransactionBase(order.TransactionInfo.TransactionId))
                        .Execute();

                    return Task.FromResult(new ApiResult
                    {
                        TransactionInfo = new TransactionInfoUpdate
                        {
                            TransactionId = response.PaymentKey,
                            PaymentStatus = PaymentStatus.Cancelled,
                        },
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Buckaroo - CancelPayment");
            }

            return Task.FromResult(ApiResult.Empty);
        }

    }
}
