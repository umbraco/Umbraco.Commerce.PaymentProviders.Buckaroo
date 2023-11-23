using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions
{
    internal static class BuckarooSettingBaseExtensions
    {
        /// <summary>
        /// Get Buckaroo api credentials from backoffice settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>Live credentials or test credentials depends on <see cref="BuckarooSettingsBase.IsTestMode"/> value.</returns>
        public static BuckarooApiCredentials GetApiCredentials(this BuckarooSettingsBase settings)
        {
            if (settings.IsTestMode)
            {
                return new BuckarooApiCredentials
                {
                    WebsiteKey = settings.TestWebsiteKey,
                    SecretKey = settings.TestApiKey,
                    IsLive = !settings.IsTestMode,
                };
            }

            return new BuckarooApiCredentials
            {
                WebsiteKey = settings.LiveWebsiteKey,
                SecretKey = settings.LiveApiKey,
                IsLive = !settings.IsTestMode,
            };
        }
    }
}
