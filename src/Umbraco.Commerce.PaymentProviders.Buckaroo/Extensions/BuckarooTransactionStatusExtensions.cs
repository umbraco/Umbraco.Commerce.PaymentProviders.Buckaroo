using System;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions
{
    public static class BuckarooTransactionStatusExtensions
    {
        public static PaymentStatus ToPaymentStatus(this int buckarooStatusCode)
        {
            ArgumentNullException.ThrowIfNull(buckarooStatusCode);

            return buckarooStatusCode switch
            {
                BuckarooSdk.Constants.Status.Success => PaymentStatus.Captured,
                BuckarooSdk.Constants.Status.CanceledByMerchant or BuckarooSdk.Constants.Status.CanceledByUser => PaymentStatus.Cancelled,

                // Not final: Buckaroo is waiting for the consumer, a third party or its own processing.
                BuckarooSdk.Constants.Status.PendingInput
                    or BuckarooSdk.Constants.Status.PendingProcessing
                    or BuckarooSdk.Constants.Status.WaitingForConsumer
                    or BuckarooSdk.Constants.Status.OnHold => PaymentStatus.PendingExternalSystem,
                _ => PaymentStatus.Error,
            };
        }
    }
}
