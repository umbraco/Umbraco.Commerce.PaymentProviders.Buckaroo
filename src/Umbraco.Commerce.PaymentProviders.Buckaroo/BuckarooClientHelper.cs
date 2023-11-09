using System.Globalization;
using BuckarooSdk;
using BuckarooSdk.Base;
using BuckarooSdk.DataTypes;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
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
            BuckarooAuthenticateRequestInput input = GetAuthenticateRequestParams(settings);
            return new SdkClient()
                .CreateRequest()
                .Authenticate(input.WebsiteKey, input.ApiKey, input.IsLive, CultureInfo.CurrentCulture);
        }

        private static BuckarooAuthenticateRequestInput GetAuthenticateRequestParams(BuckarooSettingsBase settings)
        {
            if (settings.IsTestMode)
            {
                return new BuckarooAuthenticateRequestInput
                {
                    WebsiteKey = settings.TestWebsiteKey,
                    ApiKey = settings.TestApiKey,
                    IsLive = !settings.IsTestMode,
                    Channel = ChannelEnum.Web,
                    CultureInfo = CultureInfo.CurrentCulture,
                };
            }

            return new BuckarooAuthenticateRequestInput
            {
                WebsiteKey = settings.WebsiteKey,
                ApiKey = settings.ApiKey,
                IsLive = !settings.IsTestMode,
                Channel = ChannelEnum.Web,
                CultureInfo = CultureInfo.CurrentCulture,
            };
        }
    }
}
