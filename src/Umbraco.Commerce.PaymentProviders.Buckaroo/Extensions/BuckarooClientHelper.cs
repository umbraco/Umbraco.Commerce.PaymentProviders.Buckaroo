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
        /// <returns></returns>
        public static AuthenticatedRequest GetAuthenticatedRequest(BuckarooSettingsBase settings)
        {
            BuckarooApiCredentials input = settings.GetApiCredentials();
            return new SdkClient()
                .CreateRequest()
                .Authenticate(input.WebsiteKey, input.SecretKey, input.IsLive, CultureInfo.CurrentCulture);
        }
    }
}
