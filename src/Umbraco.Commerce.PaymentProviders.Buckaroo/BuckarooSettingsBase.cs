using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    public class BuckarooSettingsBase
    {
        [PaymentProviderSetting(Name = "Continue URL", Description = "The URL to continue to after this provider has done processing. eg: /continue/", SortOrder = 100)]
        public string ContinueUrl { get; set; } = string.Empty;

        [PaymentProviderSetting(Name = "Cancel URL", Description = "The URL to return to if the payment attempt is canceled. eg: /cancel/", SortOrder = 200)]
        public string CancelUrl { get; set; } = string.Empty;

        [PaymentProviderSetting(Name = "Error URL", Description = "The URL to return to if the payment attempt errors. eg: /error/", SortOrder = 300)]
        public string ErrorUrl { get; set; } = string.Empty;

        [PaymentProviderSetting(Name = "Website key", Description = "The website key, which can be found here: https://plaza.buckaroo.nl/Configuration/WebSite/Index/", SortOrder = 400)]
        public string LiveWebsiteKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets secret key.
        /// </summary>
        [PaymentProviderSetting(Name = "Secret key", Description = "The secret key, which can be found here: https://plaza.buckaroo.nl/Configuration/Merchant/SecretKey", SortOrder = 500)]
        public string LiveApiKey { get; set; } = string.Empty;

        [PaymentProviderSetting(Name = "Webhook hostname", Description = "Set this field to the host you want to receive push message from Buckaroo. Enter hostname only. eg: 'umbraco.com'", SortOrder = 650)]
        public string LiveWebhookHostname { get; set; } = string.Empty;

        [PaymentProviderSetting(Name = "Enable test mode", Description = "Set whether to process payments in test mode", SortOrder = 10000)]
        public bool IsTestMode { get; set; }

        [PaymentProviderSetting(Name = "Website key for test mode", Description = "", SortOrder = 600)]
        public string TestWebsiteKey { get; set; } = string.Empty;

        [PaymentProviderSetting(Name = "Secret key for test mode", Description = "", SortOrder = 650)]
        public string TestApiKey { get; set; } = string.Empty;

        [PaymentProviderSetting(Name = "Webhook hostname for test mode", Description = "This field is used when you want to test webhook. eg: 'localhost:8888' or 'umbraco.com'", SortOrder = 650)]
        public string TestWebhookHostname { get; set; } = string.Empty;
    }
}
