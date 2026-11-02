using System.Text;
using BuckarooSdk.Connection;
using FluentAssertions;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.UnitTests
{
    public class BuckarooWebhookSignatureTests
    {
        private const string WebsiteKey = "TestWebsiteKey";
        private const string SecretKey = "TestSecretKey";
        private const string PathAndQuery = "/umbraco/commerce/payment/api/v1/callback/buckaroo-onetime-payment/0d6dc6a6-7c3b-4b6c-9c1e-1f0b6f1f2a3b/ORDER-01/ABC123";

        private static readonly byte[] Body = Encoding.UTF8.GetBytes("{\"Transaction\":{\"Key\":\"ABC\"}}");

        [Fact]
        public void EncodedUri_Uses_Request_Host_When_No_Override()
        {
            string actual = BuckarooWebhookHelper.GetEncodedWebhookUri(string.Empty, "Shop.Example.com", PathAndQuery);

            actual.Should().StartWith("shop.example.com%2fumbraco%2fcommerce%2fpayment%2fapi%2fv1%2fcallback%2f");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void EncodedUri_Ignores_Empty_Override(string? hostnameOverride)
        {
            BuckarooWebhookHelper.GetEncodedWebhookUri(hostnameOverride, "shop.example.com", "/a")
                .Should().Be("shop.example.com%2fa");
        }

        [Fact]
        public void EncodedUri_Uses_Override_When_Set()
        {
            BuckarooWebhookHelper.GetEncodedWebhookUri("Public.Example.com", "localhost", "/a?b=C")
                .Should().Be("public.example.com%2fa%3fb%3dc");
        }

        [Fact]
        public void Signature_From_Buckaroo_Verifies()
        {
            string uri = BuckarooWebhookHelper.GetEncodedWebhookUri(null, "shop.example.com", PathAndQuery);
            string header = CreateAuthorizationHeader(Body, "POST", uri, SecretKey);

            new SignatureCalculationService().VerifySignature(Body, "POST", uri, SecretKey, header).Should().BeTrue();
        }

        [Fact]
        public void Signature_Fails_When_Body_Changes()
        {
            string uri = BuckarooWebhookHelper.GetEncodedWebhookUri(null, "shop.example.com", PathAndQuery);
            string header = CreateAuthorizationHeader(Body, "POST", uri, SecretKey);
            byte[] tampered = Encoding.UTF8.GetBytes("{\"Transaction\":{\"Key\":\"XYZ\"}}");

            new SignatureCalculationService().VerifySignature(tampered, "POST", uri, SecretKey, header).Should().BeFalse();
        }

        [Fact]
        public void Signature_Fails_With_Wrong_Secret()
        {
            string uri = BuckarooWebhookHelper.GetEncodedWebhookUri(null, "shop.example.com", PathAndQuery);
            string header = CreateAuthorizationHeader(Body, "POST", uri, "OtherSecret");

            new SignatureCalculationService().VerifySignature(Body, "POST", uri, SecretKey, header).Should().BeFalse();
        }

        [Fact]
        public void Signature_Fails_When_Signed_For_Another_Host()
        {
            string signedUri = BuckarooWebhookHelper.GetEncodedWebhookUri(null, "public.example.com", PathAndQuery);
            string header = CreateAuthorizationHeader(Body, "POST", signedUri, SecretKey);
            string receivedUri = BuckarooWebhookHelper.GetEncodedWebhookUri(null, "internal-host", PathAndQuery);

            new SignatureCalculationService().VerifySignature(Body, "POST", receivedUri, SecretKey, header).Should().BeFalse();
        }

        [Fact]
        public void Signature_Verifies_When_Override_Matches_Signed_Host()
        {
            string signedUri = BuckarooWebhookHelper.GetEncodedWebhookUri(null, "public.example.com", PathAndQuery);
            string header = CreateAuthorizationHeader(Body, "POST", signedUri, SecretKey);
            string receivedUri = BuckarooWebhookHelper.GetEncodedWebhookUri("public.example.com", "internal-host", PathAndQuery);

            new SignatureCalculationService().VerifySignature(Body, "POST", receivedUri, SecretKey, header).Should().BeTrue();
        }

        // Buckaroo sends "Authorization: hmac {websiteKey}:{signature}:{nonce}:{timestamp}".
        private static string CreateAuthorizationHeader(byte[] body, string method, string encodedUri, string secretKey)
            => "hmac " + new SignatureCalculationService().CalculateSignature(body, method, "1700000000", "nonce123", encodedUri, WebsiteKey, secretKey);
    }
}
