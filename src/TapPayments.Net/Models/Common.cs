using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TapPayments.Models
{
    /// <summary>
    /// A customer attached to a charge, authorization, or stored on its own.
    /// </summary>
    public class TapCustomer
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

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

    public class TapPhone
    {
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("number")]
        public string Number { get; set; }
    }

    /// <summary>
    /// The payment source: a token id, a saved-card token, an authorize id,
    /// or a hosted-checkout source id such as src_card / src_all / src_kw.knet.
    /// </summary>
    public class TapSource
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    public class TapReference
    {
        [JsonPropertyName("transaction")]
        public string Transaction { get; set; }

        [JsonPropertyName("order")]
        public string Order { get; set; }

        [JsonPropertyName("gateway")]
        public string Gateway { get; set; }

        [JsonPropertyName("payment")]
        public string Payment { get; set; }
    }

    public class TapRedirect
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class TapPost
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class TapReceipt
    {
        [JsonPropertyName("email")]
        public bool Email { get; set; }

        [JsonPropertyName("sms")]
        public bool Sms { get; set; }
    }

    /// <summary>
    /// Auto capture / void behavior for an authorization.
    /// </summary>
    public class TapAuto
    {
        /// <summary>"CAPTURE" or "VOID".</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>Hours after creation (1-168).</summary>
        [JsonPropertyName("time")]
        public int Time { get; set; }
    }

    public class TapTransaction
    {
        /// <summary>Millisecond epoch.</summary>
        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }
    }

    public class TapResponseInfo
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class TapSourceInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonPropertyName("payment_type")]
        public string PaymentType { get; set; }
    }

    /// <summary>
    /// Error payload returned by the Tap API.
    /// </summary>
    public class TapApiError
    {
        [JsonPropertyName("errors")]
        public List<TapErrorDetail> Errors { get; set; }
    }

    public class TapErrorDetail
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
