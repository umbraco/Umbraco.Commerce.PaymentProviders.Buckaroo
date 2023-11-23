namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks
{
    internal class BuckarooApiCredentials
    {
        public required string WebsiteKey { get; set; }

        public required string SecretKey { get; set; }

        public bool IsLive { get; set; }
    }
}
