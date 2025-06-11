namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks
{
    /// <summary>
    /// Represents Buckaroo webhook POST data. For details explanations, visit: https://docs.buckaroo.io/docs/push-messages.
    /// </summary>
    internal class BuckarooWebhookResponse
    {
        public BuckarooWebhookTransaction? Transaction { get; set; }
    }
}
