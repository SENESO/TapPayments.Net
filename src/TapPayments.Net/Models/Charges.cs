using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TapPayments.Models
{
    /// <summary>
    /// Request to create a charge.
    /// </summary>
    public class CreateChargeRequest
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

        [JsonPropertyName("statement_descriptor")]
        public string StatementDescriptor { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonPropertyName("reference")]
        public TapReference Reference { get; set; }

        [JsonPropertyName("receipt")]
        public TapReceipt Receipt { get; set; }

        [JsonPropertyName("customer")]
        public TapCustomer Customer { get; set; }

        [JsonPropertyName("source")]
        public TapSource Source { get; set; }

        [JsonPropertyName("post")]
        public TapPost Post { get; set; }

        [JsonPropertyName("redirect")]
        public TapRedirect Redirect { get; set; }
    }

    /// <summary>
    /// A Tap charge.
    /// </summary>
    public class TapCharge
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

        [JsonPropertyName("threeDSecure")]
        public bool ThreeDSecure { get; set; }

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
        public TapSourceInfo Source { get; set; }

        [JsonPropertyName("transaction")]
        public TapTransaction Transaction { get; set; }

        [JsonPropertyName("response")]
        public TapResponseInfo Response { get; set; }

        [JsonPropertyName("redirect")]
        public TapRedirect Redirect { get; set; }

        [JsonPropertyName("post")]
        public TapPost Post { get; set; }

        /// <summary>
        /// True when the money is actually captured.
        /// </summary>
        [JsonIgnore]
        public bool IsCaptured => Status == TapChargeStatus.Captured;

        /// <summary>
        /// True while the customer is still on the payment page / 3DS.
        /// </summary>
        [JsonIgnore]
        public bool IsPendingRedirect => Status == TapChargeStatus.Initiated;
    }

    /// <summary>
    /// Known charge statuses from the Tap API.
    /// </summary>
    public static class TapChargeStatus
    {
        public const string Initiated = "INITIATED";
        public const string Captured = "CAPTURED";
        public const string Authorized = "AUTHORIZED";
        public const string Failed = "FAILED";
        public const string Declined = "DECLINED";
        public const string Cancelled = "CANCELLED";
        public const string Abandoned = "ABANDONED";
        public const string Void = "VOID";
        public const string TimedOut = "TIMEDOUT";
        public const string Restricted = "RESTRICTED";
        public const string Unknown = "UNKNOWN";
    }

    /// <summary>
    /// Well-known hosted-checkout source ids.
    /// </summary>
    public static class TapSources
    {
        public const string Card = "src_card";
        public const string All = "src_all";
        public const string Knet = "src_kw.knet";
        public const string Mada = "src_sa.mada";
        public const string Benefit = "src_bh.benefit";
        public const string OmanNet = "src_om.omannet";
    }

    /// <summary>
    /// Filters for listing charges.
    /// </summary>
    public class ChargeListRequest
    {
        [JsonPropertyName("period")]
        public TapPeriod Period { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("starting_after")]
        public string StartingAfter { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }
    }

    public class TapPeriod
    {
        [JsonPropertyName("date_from")]
        public string DateFrom { get; set; }

        [JsonPropertyName("date_to")]
        public string DateTo { get; set; }
    }

    public class ChargeListResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("data")]
        public List<TapCharge> Data { get; set; }

        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }
    }
}
