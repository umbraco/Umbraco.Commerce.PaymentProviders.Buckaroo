using System.Globalization;
using BuckarooSdk.DataTypes;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    internal class BuckarooAuthenticateRequestInput
    {
        public required string WebsiteKey { get; set; }

        public required string ApiKey { get; set; }

        public bool IsLive { get; set; }

        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;

        public ChannelEnum Channel { get; set; } = ChannelEnum.Web;
    }
}
