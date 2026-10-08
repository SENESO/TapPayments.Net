using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TapPayments.Models
{
    /// <summary>
    /// Request to refund a captured charge (full or partial).
    /// </summary>
    public class CreateRefundRequest
    {
        [JsonPropertyName("charge_id")]
        public string ChargeId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonPropertyName("reference")]
        public TapReference Reference { get; set; }

        [JsonPropertyName("post")]
        public TapPost Post { get; set; }
    }

    /// <summary>
    /// A Tap refund.
    /// </summary>
    public class TapRefund
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("live_mode")]
        public bool LiveMode { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("charge_id")]
        public string ChargeId { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonPropertyName("reference")]
        public TapReference Reference { get; set; }

        /// <summary>Millisecond epoch (refunds have no transaction object).</summary>
        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("post")]
        public TapPost Post { get; set; }
    }

    /// <summary>
    /// Request to create a stored customer.
    /// </summary>
    public class CreateCustomerRequest
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("middle_name")]
        public string MiddleName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public TapPhone Phone { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// A stored Tap customer.
    /// </summary>
    public class TapCustomerRecord
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("live_mode")]
        public bool LiveMode { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("middle_name")]
        public string MiddleName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("phone")]
        public TapPhone Phone { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }
    }

    /// <summary>
    /// Request to tokenize card details into a single-use token.
    /// Tokens are single-use and expire within minutes — never store or reuse them.
    /// </summary>
    public class CreateTokenRequest
    {
        [JsonPropertyName("card")]
        public TapCardDetails Card { get; set; }
    }

    public class TapCardDetails
    {
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("exp_month")]
        public int ExpMonth { get; set; }

        [JsonPropertyName("exp_year")]
        public int ExpYear { get; set; }

        [JsonPropertyName("cvc")]
        public string Cvc { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// A single-use Tap token.
    /// </summary>
    public class TapToken
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("used")]
        public bool Used { get; set; }
    }

    /// <summary>
    /// Request to tokenize a saved card for reuse.
    /// A saved card.id can never be used as source.id directly — tokenize it first,
    /// then pass the token id with the linked customer.id.
    /// </summary>
    public class CreateTokenFromSavedCardRequest
    {
        [JsonPropertyName("saved_card")]
        public TapSavedCard SavedCard { get; set; }
    }

    public class TapSavedCard
    {
        [JsonPropertyName("card_id")]
        public string CardId { get; set; }

        [JsonPropertyName("customer_id")]
        public string CustomerId { get; set; }
    }
}
