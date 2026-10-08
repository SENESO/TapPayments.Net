using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TapPayments.Models
{
    /// <summary>
    /// Request to create an authorization (hold funds without capturing).
    /// Credit cards only.
    /// </summary>
    public class CreateAuthorizationRequest
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("threeDSecure")]
        public bool ThreeDSecure { get; set; } = true;

        [JsonPropertyName("save_card")]
        public bool SaveCard { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonPropertyName("reference")]
        public TapReference Reference { get; set; }

        [JsonPropertyName("customer")]
        public TapCustomer Customer { get; set; }

        [JsonPropertyName("source")]
        public TapSource Source { get; set; }

        /// <summary>
        /// Auto capture/void behavior, e.g. new TapAuto { Type = "VOID", Time = 48 }.
        /// Only works when source.id is a token.
        /// </summary>
        [JsonPropertyName("auto")]
        public TapAuto Auto { get; set; }

        [JsonPropertyName("post")]
        public TapPost Post { get; set; }

        [JsonPropertyName("redirect")]
        public TapRedirect Redirect { get; set; }
    }

    /// <summary>
    /// A Tap authorization.
    /// </summary>
    public class TapAuthorization
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

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonPropertyName("reference")]
        public TapReference Reference { get; set; }

        [JsonPropertyName("customer")]
        public TapCustomer Customer { get; set; }

        [JsonPropertyName("source")]
        public TapSourceInfo Source { get; set; }

        [JsonPropertyName("transaction")]
        public TapTransaction Transaction { get; set; }

        [JsonPropertyName("response")]
        public TapResponseInfo Response { get; set; }

        [JsonPropertyName("redirect")]
        public TapRedirect Redirect { get; set; }

        [JsonPropertyName("post")]
        public TapPost Post { get; set; }

        [JsonIgnore]
        public bool IsAuthorized => Status == TapChargeStatus.Authorized;
    }
}
