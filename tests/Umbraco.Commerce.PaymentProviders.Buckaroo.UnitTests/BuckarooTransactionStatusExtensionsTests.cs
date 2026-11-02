using FluentAssertions;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.UnitTests
{
    public class BuckarooTransactionStatusExtensionsTests
    {
        [Fact]
        public void Success_Maps_To_Captured()
        {
            BuckarooSdk.Constants.Status.Success.ToPaymentStatus().Should().Be(PaymentStatus.Captured);
        }

        [Theory]
        [InlineData(BuckarooSdk.Constants.Status.CanceledByUser)]
        [InlineData(BuckarooSdk.Constants.Status.CanceledByMerchant)]
        public void Cancelled_Codes_Map_To_Cancelled(int code)
        {
            code.ToPaymentStatus().Should().Be(PaymentStatus.Cancelled);
        }

        // Pending codes (790-793) map to Error too. This pins the current behaviour; see Follow-ups in docs/upgrades/v19.md.
        [Theory]
        [InlineData(BuckarooSdk.Constants.Status.Failed)]
        [InlineData(BuckarooSdk.Constants.Status.FailedValidation)]
        [InlineData(BuckarooSdk.Constants.Status.TechnicalError)]
        [InlineData(BuckarooSdk.Constants.Status.Declined)]
        [InlineData(BuckarooSdk.Constants.Status.PendingInput)]
        [InlineData(BuckarooSdk.Constants.Status.PendingProcessing)]
        [InlineData(BuckarooSdk.Constants.Status.WaitingForConsumer)]
        [InlineData(BuckarooSdk.Constants.Status.OnHold)]
        [InlineData(0)]
        public void Other_Codes_Map_To_Error(int code)
        {
            code.ToPaymentStatus().Should().Be(PaymentStatus.Error);
        }
    }
}
