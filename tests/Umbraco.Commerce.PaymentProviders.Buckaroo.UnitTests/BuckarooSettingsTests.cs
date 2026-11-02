using FluentAssertions;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.UnitTests
{
    public class BuckarooSettingsTests
    {
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ApiCredentials_Map_From_Settings(bool isTestMode, bool expectedIsLive)
        {
            BuckarooOneTimeSettings settings = new() { WebsiteKey = "web", ApiKey = "secret", IsTestMode = isTestMode };

            BuckarooApiCredentials actual = settings.GetApiCredentials();

            actual.WebsiteKey.Should().Be("web");
            actual.SecretKey.Should().Be("secret");
            actual.IsLive.Should().Be(expectedIsLive);
        }

        [Theory]
        [InlineData("", "secret")]
        [InlineData("web", "")]
        [InlineData(" ", " ")]
        public void Client_Requires_WebsiteKey_And_SecretKey(string websiteKey, string apiKey)
        {
            BuckarooOneTimeSettings settings = new() { WebsiteKey = websiteKey, ApiKey = apiKey };

            Action act = () => BuckarooClientHelper.GetAuthenticatedRequest(settings);

            act.Should().Throw<BuckarooInvalidSettingsException>();
        }

        [Fact]
        public void Client_Is_Created_With_Both_Keys()
        {
            BuckarooOneTimeSettings settings = new() { WebsiteKey = "web", ApiKey = "secret", IsTestMode = true };

            BuckarooClientHelper.GetAuthenticatedRequest(settings).Should().NotBeNull();
        }
    }
}
