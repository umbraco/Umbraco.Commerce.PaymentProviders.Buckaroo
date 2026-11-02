using System;
using System.Net;
using System.Text.Json;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks
{
    internal static class BuckarooWebhookHelper
    {
        public static BuckarooWebhookTransaction ParseDataFromBytes(byte[] data)
        {
            string jsonContent = System.Text.Encoding.UTF8.GetString(data);
            BuckarooWebhookTransaction buckarooEvent = JsonSerializer.Deserialize<BuckarooWebhookResponse?>(jsonContent)?.Transaction ?? throw new NotImplementedException("Unable to parse buckaroo push message to object");
            return buckarooEvent;
        }

        /// <summary>
        /// Builds the URI that Buckaroo signs a push message with: the URL-encoded host, path and query, in lower case.
        /// </summary>
        /// <param name="hostnameOverride">The host name set in the provider settings, used instead of the request's host when it isn't empty.</param>
        /// <param name="requestHost">The host of the incoming request.</param>
        /// <param name="encodedPathAndQuery">The encoded path and query of the incoming request.</param>
        public static string GetEncodedWebhookUri(string? hostnameOverride, string requestHost, string encodedPathAndQuery)
        {
            string webhookHostname = !string.IsNullOrWhiteSpace(hostnameOverride) ? hostnameOverride : requestHost;

#pragma warning disable CA1308 // Do not normalize strings to uppercase because Buckaroo asks for lowercase string
            return WebUtility.UrlEncode(webhookHostname + encodedPathAndQuery).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
        }
    }
}
