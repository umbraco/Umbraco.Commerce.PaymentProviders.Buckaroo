using System;
using BuckarooSdk.DataTypes.Response.Status;
using Umbraco.Commerce.Core.Models;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    public static class BuckarooTransactionStatusExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        public static PaymentStatus ToPaymentStatus(this StatusCode buckarooStatusCode)
        {
            ArgumentNullException.ThrowIfNull(nameof(buckarooStatusCode));

            return buckarooStatusCode.Code switch
            {
                BuckarooSdk.Constants.Status.Success => PaymentStatus.Captured,
                BuckarooSdk.Constants.Status.CanceledByMerchant or BuckarooSdk.Constants.Status.CanceledByUser => PaymentStatus.Cancelled,
                _ => PaymentStatus.Error,
            };
        }
    }
}
