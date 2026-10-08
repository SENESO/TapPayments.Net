using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TapPayments.Models;

namespace TapPayments
{
    /// <summary>
    /// Client for the Tap Payments API (v2).
    /// </summary>
    public class TapClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };

        private readonly TapClientOptions _options;
        private readonly HttpClient _http;

        public TapClient(TapClientOptions options, HttpClient httpClient = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.Validate();

            _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds) };

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.SecretKey);
        }

        public TapClient(string secretKey, HttpClient httpClient = null)
            : this(new TapClientOptions { SecretKey = secretKey }, httpClient)
        {
        }

        // ---------- Charges ----------

        /// <summary>
        /// Creates a charge. For 3DS / local schemes the response has status INITIATED
        /// and a transaction.url — redirect the customer there.
        /// </summary>
        public Task<TapCharge> CreateChargeAsync(CreateChargeRequest request, CancellationToken ct = default)
            => PostAsync<TapCharge>("charges", request, ct);

        /// <summary>
        /// Retrieves a charge by id.
        /// </summary>
        public Task<TapCharge> RetrieveChargeAsync(string chargeId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(chargeId)) throw new ArgumentException("chargeId is required.", nameof(chargeId));
            return GetAsync<TapCharge>($"charges/{chargeId}", ct);
        }

        /// <summary>
        /// Updates a charge's description/metadata.
        /// </summary>
        public Task<TapCharge> UpdateChargeAsync(string chargeId, UpdateChargeRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(chargeId)) throw new ArgumentException("chargeId is required.", nameof(chargeId));
            return PutAsync<TapCharge>($"charges/{chargeId}", request, ct);
        }

        /// <summary>
        /// Lists charges with optional filters.
        /// </summary>
        public Task<ChargeListResponse> ListChargesAsync(ChargeListRequest request, CancellationToken ct = default)
            => PostAsync<ChargeListResponse>("charges/list", request ?? new ChargeListRequest(), ct);

        /// <summary>
        /// Captures a previously authorized amount by creating a charge against the authorize id.
        /// </summary>
        public Task<TapCharge> CaptureAuthorizationAsync(string authorizeId, decimal? amount = null, string currency = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(authorizeId)) throw new ArgumentException("authorizeId is required.", nameof(authorizeId));
            var request = new CreateChargeRequest
            {
                Source = new TapSource { Id = authorizeId }
            };
            if (amount.HasValue) request.Amount = amount.Value;
            if (!string.IsNullOrWhiteSpace(currency)) request.Currency = currency;
            return PostAsync<TapCharge>("charges", request, ct);
        }

        // ---------- Authorize ----------

        /// <summary>
        /// Creates an authorization (holds funds without capturing). Credit cards only.
        /// </summary>
        public Task<TapAuthorization> CreateAuthorizationAsync(CreateAuthorizationRequest request, CancellationToken ct = default)
            => PostAsync<TapAuthorization>("authorize", request, ct);

        /// <summary>
        /// Retrieves an authorization by id.
        /// </summary>
        public Task<TapAuthorization> RetrieveAuthorizationAsync(string authorizeId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(authorizeId)) throw new ArgumentException("authorizeId is required.", nameof(authorizeId));
            return GetAsync<TapAuthorization>($"authorize/{authorizeId}", ct);
        }

        /// <summary>
        /// Voids an existing authorization.
        /// </summary>
        public Task<TapAuthorization> VoidAuthorizationAsync(string authorizeId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(authorizeId)) throw new ArgumentException("authorizeId is required.", nameof(authorizeId));
            return PostAsync<TapAuthorization>($"authorize/{authorizeId}/void", new { }, ct);
        }

        // ---------- Refunds ----------

        /// <summary>
        /// Creates a refund (full or partial) against a captured charge.
        /// </summary>
        public Task<TapRefund> CreateRefundAsync(CreateRefundRequest request, CancellationToken ct = default)
            => PostAsync<TapRefund>("refunds", request, ct);

        /// <summary>
        /// Retrieves a refund by id.
        /// </summary>
        public Task<TapRefund> RetrieveRefundAsync(string refundId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(refundId)) throw new ArgumentException("refundId is required.", nameof(refundId));
            return GetAsync<TapRefund>($"refunds/{refundId}", ct);
        }

        // ---------- Customers ----------

        /// <summary>
        /// Creates a stored customer.
        /// </summary>
        public Task<TapCustomerRecord> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken ct = default)
            => PostAsync<TapCustomerRecord>("customers", request, ct);

        /// <summary>
        /// Retrieves a customer by id.
        /// </summary>
        public Task<TapCustomerRecord> RetrieveCustomerAsync(string customerId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(customerId)) throw new ArgumentException("customerId is required.", nameof(customerId));
            return GetAsync<TapCustomerRecord>($"customers/{customerId}", ct);
        }

        // ---------- Tokens ----------

        /// <summary>
        /// Tokenizes card details into a single-use token.
        /// Tokens expire within minutes — never store or reuse them.
        /// </summary>
        public Task<TapToken> CreateTokenAsync(CreateTokenRequest request, CancellationToken ct = default)
            => PostAsync<TapToken>("tokens", request, ct);

        /// <summary>
        /// Tokenizes a saved card for reuse. A saved card.id can never be used as
        /// source.id directly — use the returned token id instead.
        /// </summary>
        public Task<TapToken> CreateTokenFromSavedCardAsync(CreateTokenFromSavedCardRequest request, CancellationToken ct = default)
            => PostAsync<TapToken>("tokens", request, ct);

        // ---------- HTTP plumbing ----------

        private async Task<T> GetAsync<T>(string path, CancellationToken ct)
        {
            using (var response = await _http.GetAsync(BuildUrl(path), ct).ConfigureAwait(false))
                return await ReadAsync<T>(response).ConfigureAwait(false);
        }

        private async Task<T> PostAsync<T>(string path, object body, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await _http.PostAsync(BuildUrl(path), content, ct).ConfigureAwait(false))
                return await ReadAsync<T>(response).ConfigureAwait(false);
        }

        private async Task<T> PutAsync<T>(string path, object body, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await _http.PutAsync(BuildUrl(path), content, ct).ConfigureAwait(false))
                return await ReadAsync<T>(response).ConfigureAwait(false);
        }

        private string BuildUrl(string path)
        {
            var baseUrl = (_options.BaseUrl ?? "https://api.tap.company/v2").TrimEnd('/');
            return baseUrl + "/" + path.TrimStart('/');
        }

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var (code, description) = ParseError(body);
                throw new TapApiException(
                    $"Tap API error {(int)response.StatusCode} {response.ReasonPhrase}" +
                    (description != null ? $": {description}" : string.Empty),
                    response.StatusCode, code, description);
            }

            if (string.IsNullOrWhiteSpace(body))
                throw new TapApiException("Tap API returned an empty response.", response.StatusCode);

            try
            {
                return JsonSerializer.Deserialize<T>(body, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new TapApiException("Failed to parse Tap API response.", response.StatusCode, inner: ex);
            }
        }

        private static (string code, string description) ParseError(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return (null, null);
            try
            {
                var error = JsonSerializer.Deserialize<TapApiError>(body, JsonOptions);
                var first = error?.Errors?.FirstOrDefault();
                return (first?.Code, first?.Description);
            }
            catch
            {
                return (null, null);
            }
        }
    }

    /// <summary>
    /// Updatable charge fields.
    /// </summary>
    public class UpdateChargeRequest
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("metadata")]
        public System.Collections.Generic.Dictionary<string, string> Metadata { get; set; }

        [JsonPropertyName("reference")]
        public TapReference Reference { get; set; }

        [JsonPropertyName("receipt")]
        public TapReceipt Receipt { get; set; }
    }
}
