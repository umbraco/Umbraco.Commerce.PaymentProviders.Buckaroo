using System;
using System.Globalization;
using System.Text;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks.Exceptions
{
    public class BuckarooWebhookInvalidSignatureException : Exception
    {
        private static readonly CompositeFormat _messageFormat = CompositeFormat.Parse("Buckaroo - Invalid signature. OrderNumber: '{0}', CartNumber: '{1}'. encodedUri: '{2}'");

        public BuckarooWebhookInvalidSignatureException(
            string orderNumber,
            string cartNumber,
            string encodedWebhookPath)
            : base(string.Format(CultureInfo.InvariantCulture, _messageFormat, orderNumber, cartNumber, encodedWebhookPath))
        {
        }

        public BuckarooWebhookInvalidSignatureException(string message) : base(message)
        {
        }

        public BuckarooWebhookInvalidSignatureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
