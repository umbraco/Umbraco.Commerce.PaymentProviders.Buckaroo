using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    public class BuckarooSettingsBase
    {
        [PaymentProviderSetting]
        public string ContinueUrl { get; set; } = string.Empty;

        [PaymentProviderSetting]
        public string CancelUrl { get; set; } = string.Empty;

        [PaymentProviderSetting]
        public string ErrorUrl { get; set; } = string.Empty;

        [PaymentProviderSetting]
        public string WebsiteKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets secret key.
        /// </summary>
        [PaymentProviderSetting]
        public string ApiKey { get; set; } = string.Empty;

        [PaymentProviderSetting]
        public string WebhookHostnameOverwrite { get; set; } = string.Empty;

        [PaymentProviderSetting]
        public bool IsTestMode { get; set; }
    }
}
