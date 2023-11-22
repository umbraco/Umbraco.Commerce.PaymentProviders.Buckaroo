namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks
{
    public class BuckarooWebhookTransactionStatusCodeDetails
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
