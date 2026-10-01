using FluentAssertions;
using Umbraco.Commerce.Core.Models;
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
            actual.ServiceCode.Should().Be("ideal");
        }

        [Theory]
        [InlineData("ideal")]
        [InlineData("creditcard")]
        [InlineData("paypal")]
        public void Callback_Should_Save_Selected_Payment_Method(string serviceCode)
        {
            BuckarooWebhookTransaction transaction = new()
            {
                Key = "transaction-key",
                ServiceCode = serviceCode,
                AmountDebit = 21.6m,
                Status = new BuckarooWebhookTransactionStatusCode
                {
                    Code = new BuckarooWebhookTransactionStatusCodeDetails { Code = BuckarooSdk.Constants.Status.Success },
                },
            };

            var result = BuckarooOneTimePaymentProvider.CreateCallbackResult(transaction);

            result.TransactionInfo.TransactionId.Should().Be("transaction-key");
            result.TransactionInfo.PaymentStatus.Should().Be(PaymentStatus.Captured);
            result.TransactionInfo.AmountAuthorized.Should().Be(21.6m);
            result.MetaData.Should().ContainKey("buckarooPaymentMethod").WhoseValue.Should().Be(serviceCode);
        }

        [Fact]
        public void Unsuccessful_Callback_Should_Save_Selected_Payment_Method_Without_Authorizing_Amount()
        {
            BuckarooWebhookTransaction transaction = new()
            {
                ServiceCode = "paypal",
                AmountDebit = 21.6m,
                Status = new BuckarooWebhookTransactionStatusCode
                {
                    Code = new BuckarooWebhookTransactionStatusCodeDetails { Code = BuckarooSdk.Constants.Status.CanceledByUser },
                },
            };

            var result = BuckarooOneTimePaymentProvider.CreateCallbackResult(transaction);

            result.TransactionInfo.PaymentStatus.Should().Be(PaymentStatus.Cancelled);
            result.TransactionInfo.AmountAuthorized.Should().Be(0);
            result.MetaData.Should().ContainKey("buckarooPaymentMethod").WhoseValue.Should().Be("paypal");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Missing_Service_Code_Should_Not_Create_Payment_Method_MetaData(string? serviceCode)
        {
            BuckarooOneTimePaymentProvider.GetPaymentMethodMetaData(serviceCode).Should().BeEmpty();
        }
    }
}
