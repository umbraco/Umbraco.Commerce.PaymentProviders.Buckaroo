using System;
using System.Globalization;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks.Exceptions
{
    public class BuckarooWebhookEmptyBodyException : Exception
    {
        private static readonly CompositeFormat _messageFormat = CompositeFormat.Parse("Buckaroo - Empty body content. OrderNumber: '{0}', CartNumber: '{1}'. encodedUri: '{2}'");

        public BuckarooWebhookEmptyBodyException(
            string orderNumber,
            string cartNumber,
            string webhookUri)
            : base(string.Format(CultureInfo.InvariantCulture, _messageFormat, orderNumber, cartNumber, webhookUri))
        {
        }

        public BuckarooWebhookEmptyBodyException(string message) : base(message)
        {
        }

        public BuckarooWebhookEmptyBodyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
