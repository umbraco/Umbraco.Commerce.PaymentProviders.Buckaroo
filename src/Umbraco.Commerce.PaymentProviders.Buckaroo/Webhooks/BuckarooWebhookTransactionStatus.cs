using System;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks
{
    internal class BuckarooWebhookTransactionStatus
    {
        public required BuckarooWebhookTransactionStatusCode Code { get; set; }
        public DateTime DateTime { get; set; }
    }
}
