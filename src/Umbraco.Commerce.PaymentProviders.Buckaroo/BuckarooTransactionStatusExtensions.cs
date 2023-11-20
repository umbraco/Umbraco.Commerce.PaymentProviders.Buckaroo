using System;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
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
                _ => PaymentStatus.Error,
            };
        }
    }
}
