using System.Globalization;
using BuckarooSdk;
using BuckarooSdk.Base;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions
{
    internal class BuckarooClientHelper
    {
        /// <summary>
        /// Get a new instance of <see cref="AuthenticatedRequest"/> each time it is called.
        /// </summary>
        /// <param name="settings"></param>
        /// <exception cref="BuckarooInvalidSettingsException"></exception>
        /// <returns></returns>
        public static AuthenticatedRequest GetAuthenticatedRequest(BuckarooSettingsBase settings)
        {
            BuckarooApiCredentials credentials = settings.GetApiCredentials();
            if (string.IsNullOrWhiteSpace(credentials.SecretKey) || string.IsNullOrWhiteSpace(credentials.WebsiteKey))
            {
                throw new BuckarooInvalidSettingsException();
            }

            return new SdkClient()
                .CreateRequest()
                .Authenticate(credentials.WebsiteKey, credentials.SecretKey, credentials.IsLive, CultureInfo.CurrentCulture);
        }
    }
}
