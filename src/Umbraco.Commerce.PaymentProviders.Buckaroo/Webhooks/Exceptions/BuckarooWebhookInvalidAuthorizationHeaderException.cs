using System;
using System.Globalization;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks.Exceptions
{
    public class BuckarooWebhookInvalidAuthorizationHeaderException : Exception
    {
        private static readonly CompositeFormat _messageFormat = CompositeFormat.Parse("Buckaroo - Authorization header was not found. OrderNumber: '{0}', CartNumber: '{1}'. encodedUri: '{2}'");

        public BuckarooWebhookInvalidAuthorizationHeaderException(
            string orderNumber,
            string cartNumber,
            string webhookUri)
            : base(string.Format(CultureInfo.InvariantCulture, _messageFormat, orderNumber, cartNumber, webhookUri))
        {
        }

        public BuckarooWebhookInvalidAuthorizationHeaderException(string message) : base(message)
        {
        }

        public BuckarooWebhookInvalidAuthorizationHeaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
