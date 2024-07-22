using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BuckarooSdk.Connection;
using BuckarooSdk.DataTypes;
using BuckarooSdk.DataTypes.RequestBases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks.Exceptions;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    [PaymentProvider("buckaroo-onetime-payment")]
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
                ReturnUrlError = context.Urls.ErrorUrl,
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
                BuckarooWebhookTransaction buckarooEvent = ParseWebhookData(context);
                if (!buckarooEvent.IsSuccess)
                {
                    return CallbackResult.Ok(new TransactionInfo
                    {
                        TransactionId = buckarooEvent.Key,
                        PaymentStatus = buckarooEvent.Status.Code.Code.ToPaymentStatus(),
                    });
                }

                OrderReadOnly order = context.Order;

                TransactionInfo transactionInfo = new()
                {
                    TransactionId = buckarooEvent.Key,
                    PaymentStatus = buckarooEvent.Status.Code.Code.ToPaymentStatus(),
                    AmountAuthorized = buckarooEvent.AmountDebit ?? buckarooEvent.AmountCredit ?? 0,
                };

                return CallbackResult.Ok(transactionInfo);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Buckaroo - ProcessCallbackAsync. OrderNumber: {OrderNumber}, CartNumber: {CartNumber}", context.Order.OrderNumber, context.Order.CartNumber);
            }

            return CallbackResult.BadRequest();
        }

        private BuckarooWebhookTransaction ParseWebhookData(PaymentProviderContext<BuckarooOneTimeSettings> context)
        {
            Logger.Info("Begin parsing buckaroo callback data.");

            HttpRequest request = context.HttpContext.Request;
            request.Headers.TryGetValue("Authorization", out StringValues header);
            string?[] authorizationHeaders = header.ToArray();
            if (authorizationHeaders == null || string.IsNullOrEmpty(authorizationHeaders.FirstOrDefault()))
            {
                throw new BuckarooWebhookInvalidAuthorizationHeaderException(context.Order.OrderNumber, context.Order.CartNumber, request.GetDisplayUrl());
            }

            if (request.Body == null)
            {
                throw new BuckarooWebhookEmptyBodyException(context.Order.OrderNumber, context.Order.CartNumber, request.GetDisplayUrl());
            }

#pragma warning disable CA1308 // Do not normalize strings to uppercase because Buckaroo asks for lowercase string ¯\_(ツ)_/¯
            string webhookHostname = !string.IsNullOrWhiteSpace(context.Settings.WebhookHostnameOverwrite) ? context.Settings.WebhookHostnameOverwrite : request.Host.Host;
            string encodedUri = WebUtility.UrlEncode(webhookHostname + request.GetEncodedPathAndQuery()).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            byte[] requestBody = request.Body.ToByteArray();
            BuckarooApiCredentials apiCredentials = context.Settings.GetApiCredentials();

            SignatureCalculationService signatureService = new();
            bool signatureVerified = signatureService.VerifySignature(requestBody, request.Method.ToUpperInvariant(), encodedUri, apiCredentials.SecretKey, authorizationHeaders.First());
            if (!signatureVerified)
            {
                throw new BuckarooWebhookInvalidSignatureException(context.Order.OrderNumber, context.Order.CartNumber, encodedUri);
            }

            return BuckarooWebhookHelper.ParseDataFromBytes(requestBody);
        }

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
