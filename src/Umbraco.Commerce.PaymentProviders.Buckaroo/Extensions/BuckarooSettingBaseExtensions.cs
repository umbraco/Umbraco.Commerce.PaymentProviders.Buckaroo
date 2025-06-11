using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions
{
    internal static class BuckarooSettingBaseExtensions
    {
        /// <summary>
        /// Get Buckaroo api credentials from backoffice settings.
        /// </summary>
        /// <param name="settings"></param>
        public static BuckarooApiCredentials GetApiCredentials(this BuckarooSettingsBase settings) => new BuckarooApiCredentials
        {
            WebsiteKey = settings.WebsiteKey,
            SecretKey = settings.ApiKey,
            IsLive = !settings.IsTestMode,
        };
    }
}
