using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BuckarooSdk.DataTypes;
using BuckarooSdk.DataTypes.RequestBases;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    [PaymentProvider("buckaroo-onetime-payment", "Buckaroo One Time Payment", "Buckaroo one time payment provider")]
    public class BuckarooOneTimePaymentProvider : BuckarooPaymentProviderBase<BuckarooOneTimePaymentProvider, BuckarooOneTimeSettings>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BuckarooOneTimePaymentProvider(
            UmbracoCommerceContext ctx,
            ILogger<BuckarooOneTimePaymentProvider> logger,
            IHttpContextAccessor httpContextAccessor)
            : base(ctx, logger)
        {
            _httpContextAccessor = httpContextAccessor;
        }

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
                ReturnUrl = context.Urls.ContinueUrl,
                ReturnUrlCancel = context.Urls.CancelUrl,
                ReturnUrlError = context.Urls.ErrorUrl, // ToDo: the error url is different than the provided continue and cancel urls (webhook urls),
                                                        // we probably want this from the method's arguments as well // ask Matt
                ReturnUrlReject = context.Urls.CancelUrl,
                StartRecurrent = false,
                ClientIp = new IpAddress
                {
                    // TODO: need to test on a remote server. IP on localhost always be "::1"
                    Address = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Type = InternetProtocolVersion.IPv4, // TODO: How about IPv6?
                },
                ClientUserAgent = _httpContextAccessor.HttpContext.Request.Headers["User-Agent"],
                ContinueOnIncomplete = ContinueOnIncomplete.RedirectToHTML,
                PushUrl = context.Urls.CallbackUrl,
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
            ArgumentNullException.ThrowIfNull(context);

            try
            {
                BuckarooWebhookEvent? buckarooEvent = await ParseWebhookDataAsync(context, cancellationToken).ConfigureAwait(false);
                if (buckarooEvent == null || !buckarooEvent.IsSuccess)
                {
                    // Just returns OK without finalizing the order
                    return CallbackResult.Empty;
                }

                OrderReadOnly order = context.Order;
                if (order.IsFinalized)
                {
                    return CallbackResult.Empty;
                }

                if (!order.IsFinalized)
                {
                    var transactionInfo = new TransactionInfo
                    {
                        TransactionId = buckarooEvent.PaymentId,
                        PaymentStatus = buckarooEvent.StatusCode.ToPaymentStatus(),
                        AmountAuthorized = buckarooEvent.Amount ?? 0,
                    };

                    return CallbackResult.Ok(transactionInfo);
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Buckaroo - ProcessCallbackAsync. OrderNumber: {OrderNumber}, CartNumber: {CartNumber}", context.Order.OrderNumber, context.Order.CartNumber);
            }

            return CallbackResult.BadRequest();
        }

        private static async Task<BuckarooWebhookEvent?> ParseWebhookDataAsync(PaymentProviderContext<BuckarooOneTimeSettings> context, CancellationToken cancellationToken)
        {
            if (context.Request.Content == null)
            {
                return null;
            }

            Stream stream = await context.Request.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string requestBodyContent = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                // TODO: Dinh - Maybe we need to validate the webhook signature
                //EventUtility.ValidateSignature(json, buckarooSignature, webhookSigningSecret);

                BuckarooWebhookEvent buckarooEvent = ParseWebhookBodyContent(requestBodyContent) ?? throw new NotImplementedException("Unable to parse buckaroo push message to object");
                return buckarooEvent;
            }
        }

        private static BuckarooWebhookEvent? ParseWebhookBodyContent(string queryString)
        {
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(queryString);
            var dict = nameValueCollection
                .OfType<string>()
                .ToDictionary(key => key, key => nameValueCollection[key]);

            // Transform postdata to json and deserialize
            var json = JsonConvert.SerializeObject(dict);
            return JsonConvert.DeserializeObject<BuckarooWebhookEvent?>(json);
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
                        TransactionId = order.TransactionInfo.TransactionId,
                        PaymentStatus = status.Status.Code.Code.ToPaymentStatus(),
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
                if (order.TransactionInfo.PaymentStatus == PaymentStatus.Authorized) // This condition is always false?
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
