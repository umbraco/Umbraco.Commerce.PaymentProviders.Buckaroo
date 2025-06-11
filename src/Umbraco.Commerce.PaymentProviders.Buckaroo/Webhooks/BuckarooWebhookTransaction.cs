namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks
{
    /// <summary>
    /// Represents Buckaroo webhook POST data. For details explanations, visit: https://docs.buckaroo.io/docs/push-messages.
    /// </summary>
    internal class BuckarooWebhookTransaction
    {
        /// <summary>
        /// brq_transactions. One or several transaction key(s). One key if one underlying transaction was created for the payment. Several keys if several underlying transactions are linked to the payment.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// brq_amount.
        /// </summary>
        public decimal? AmountDebit { get; set; }

        /// <summary>
        /// brq_amount_credit.
        /// </summary>
        public decimal? AmountCredit { get; set; }

        /// <summary>
        /// i.e: 'GBP'
        /// </summary>
        public string Currency { get; set; } = string.Empty;

        public string? CustomerName { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// brq_invoicenumber.
        /// </summary>
        public string Invoice { get; set; } = string.Empty;

        /// <summary>
        /// brq_payment.
        /// </summary>
        public string PaymentKey { get; set; } = string.Empty;

        /// <summary>
        /// The status code of the transaction.
        /// </summary>
        public required BuckarooWebhookTransactionStatusCode Status { get; set; }

        public bool IsTest { get; set; }

        /// <summary>
        /// brq_transaction_type.
        /// </summary>
        public string? TransactionType { get; set; }

        /// <summary>
        /// brq_transaction_method. i.e: 'paypal'.
        /// </summary>
        public string ServiceCode { get; set; } = string.Empty;

        public bool IsSuccess => Status.Code.Code == BuckarooSdk.Constants.Status.Success;
    }
}
