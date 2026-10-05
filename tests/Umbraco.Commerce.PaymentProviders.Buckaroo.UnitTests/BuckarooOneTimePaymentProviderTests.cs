using FluentAssertions;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.UnitTests
{
    public class BuckarooOneTimePaymentProviderTests
    {
        [Fact()]
        public void ParseDataFromBytes_Should_Returns_Data()
        {
            // setup
            string json = @"
                {
                  ""Transaction"": {
                    ""Key"": ""7691587684B745B685D08DF0A2C4CB9E"",
                    ""Invoice"": ""ORDER-01437-26779-SJRWY"",
                    ""ServiceCode"": ""ideal"",
                    ""Status"": {
                      ""Code"": {
                        ""Code"": 190,
                        ""Description"": ""Success""
                      },
                      ""SubCode"": {
                        ""Code"": ""C000"",
                        ""Description"": ""Success""
                      },
                      ""DateTime"": ""2023-12-08T08:29:00""
                    },
                    ""IsTest"": true,
                    ""Order"": ""ORDER-01437-26779-SJRWY"",
                    ""Currency"": ""EUR"",
                    ""AmountCredit"": 22.6,
                    ""AmountDebit"": 21.6,
                    ""TransactionType"": ""C021"",
                    ""Services"": null,
                    ""CustomParameters"": null,
                    ""AdditionalParameters"": null,
                    ""MutationType"": 1,
                    ""RelatedTransactions"": null,
                    ""IsCancelable"": false,
                    ""IssuingCountry"": null,
                    ""StartRecurrent"": false,
                    ""Recurring"": false,
                    ""CustomerName"": ""J. de Tèster"",
                    ""PayerHash"": ""93c6784fdfe3336d1fad2b0ff676d8509ab6834d882eac8263af035139c036ee4e93992502862d332c4a48c2af0fd7d67b8845067de45a420a059d49c8cb6923"",
                    ""PaymentKey"": ""90EB5506375A425480285F0EEA833182"",
                    ""Description"": ""ORDER-01437-26779-SJRWY""
                  }
                }";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

            // execute
            BuckarooWebhookTransaction? actual = BuckarooWebhookHelper.ParseDataFromBytes(data);

            // asserts
            actual.Should().NotBeNull();
            TestHelpers.AssertAllPropertiesAreNotNull(actual);
        }

        [Theory]
        [InlineData("ideal")]
        [InlineData("paypal")]
        [InlineData("visa")]
        public void CreatePaymentMethodMetaData_Should_Contain_ServiceCode(string serviceCode)
        {
            Dictionary<string, string> actual = BuckarooOneTimePaymentProvider.CreatePaymentMethodMetaData(serviceCode);

            actual.Should().ContainKey(BuckarooOneTimePaymentProvider.PaymentMethodMetaDataKey)
                .WhoseValue.Should().Be(serviceCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CreatePaymentMethodMetaData_Should_Be_Empty_When_No_ServiceCode(string? serviceCode)
        {
            BuckarooOneTimePaymentProvider.CreatePaymentMethodMetaData(serviceCode).Should().BeEmpty();
        }
    }
}
