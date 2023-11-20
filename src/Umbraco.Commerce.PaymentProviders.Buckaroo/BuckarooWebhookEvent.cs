using System;
using BuckarooSdk.Constants;
using BuckarooSdk.DataTypes.Response;
using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    /// <summary>
    /// Represents Buckaroo webhook POST data. For details explanations, visit: https://docs.buckaroo.io/docs/push-messages.
    /// </summary>
    public class BuckarooWebhookEvent
    {
        /// <summary>
        /// One or several transaction key(s). One key if one underlying transaction was created for the payment. Several keys if several underlying transactions are linked to the payment.
        /// </summary>
        [JsonProperty("brq_transactions")]
        public string PaymentId { get; set; } = string.Empty;

        [JsonProperty("brq_amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("brq_currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonProperty("brq_customer_name")]
        public string? CustomerName { get; set; }

        [JsonProperty("brq_description")]
        public string? Description { get; set; }

        [JsonProperty("brq_invoicenumber")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [JsonProperty("brq_mutationtype")]
        public MutationType MutationType { get; set; }

        [JsonProperty("brq_ordernumber")]
        public string? OrderNumber { get; set; }

        [JsonProperty("brq_payer_hash")]
        public string? PayerHash { get; set; }

        /// <summary>
        /// The Payment key. This value is only communicated if a payment method was selected.
        /// </summary>
        [JsonProperty("brq_payment")]
        public string Payment { get; set; } = string.Empty;

        /// <summary>
        /// The status code of the transaction.
        /// </summary>
        [JsonProperty("brq_statuscode")]
        public int StatusCode { get; set; }

        [JsonProperty("brq_statuscode_detail")]
        public string? StatusCodeDetail { get; set; }

        [JsonProperty("brq_statusmessage")]
        public string StatusMessage { get; set; } = string.Empty;

        [JsonProperty("brq_test")]
        public bool IsTest { get; set; }

        [JsonProperty("brq_timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("brq_transaction_method")]
        public string TransactionMethod { get; set; } = string.Empty;

        [JsonProperty("brq_transaction_type")]
        public string? TransactionType { get; set; }

        [JsonProperty("brq_websitekey")]
        public string? WebsiteKey { get; set; }

        [JsonProperty("brq_signature")]
        public string Signature { get; set; } = string.Empty;

        [JsonProperty("brq_payment_method")]
        public string? PaymentMethod { get; set; }

        [JsonProperty("brq_amount_credit")]
        public decimal AmountCredit { get; set; }

        public bool IsSuccess => StatusCode == Status.Success;
    }
}
