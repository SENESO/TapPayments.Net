using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TapPayments.Webhooks
{
    /// <summary>
    /// Validates Tap webhook requests using the official hashstring scheme:
    /// HMAC-SHA256 of a canonical field string, keyed with the secret API key,
    /// compared against the "hashstring" header.
    /// </summary>
    public static class TapWebhookValidator
    {
        private static readonly string[] ThreeDecimalCurrencies = { "BHD", "KWD", "OMR", "JOD" };

        /// <summary>
        /// Validates a webhook payload against the hashstring header value.
        /// </summary>
        /// <param name="jsonPayload">The raw JSON body Tap POSTed.</param>
        /// <param name="hashstringHeader">The value of the "hashstring" request header.</param>
        /// <param name="secretKey">Your Tap secret API key (sk_test_... / sk_live_...).</param>
        /// <param name="objectType">"charge", "authorize", "refund", or "invoice".</param>
        public static bool IsValid(string jsonPayload, string hashstringHeader, string secretKey, string objectType)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload) ||
                string.IsNullOrWhiteSpace(hashstringHeader) ||
                string.IsNullOrWhiteSpace(secretKey) ||
                string.IsNullOrWhiteSpace(objectType))
                return false;

            JsonDocument doc;
            try { doc = JsonDocument.Parse(jsonPayload); }
            catch { return false; }

            using (doc)
            {
                var root = doc.RootElement;
                var canonical = BuildCanonicalString(root, objectType.Trim().ToLowerInvariant());
                if (canonical == null) return false;

                var expected = ComputeHmacSha256Hex(secretKey, canonical);
                return ConstantTimeEquals(expected, hashstringHeader);
            }
        }

        /// <summary>
        /// Builds the canonical string for the given object type.
        /// charge / authorize / refund:
        ///   x_id{id}x_amount{amount}x_currency{currency}x_gateway_reference{gw}x_payment_reference{pay}x_status{status}x_created{created}
        /// invoice:
        ///   x_id{id}x_amount{amount}x_currency{currency}x_updated{updated}x_status{status}x_created{created}
        /// </summary>
        public static string BuildCanonicalString(JsonElement root, string objectType)
        {
            var id = GetString(root, "id");
            var currency = GetString(root, "currency");
            var status = GetString(root, "status");
            var amount = FormatAmount(GetDecimal(root, "amount"), currency);

            if (objectType == "invoice")
            {
                var updated = GetString(root, "updated");
                var created = GetString(root, "created");
                return "x_id" + id + "x_amount" + amount + "x_currency" + currency +
                       "x_updated" + updated + "x_status" + status + "x_created" + created;
            }

            if (objectType == "charge" || objectType == "authorize" || objectType == "refund")
            {
                string gateway = string.Empty, payment = string.Empty;
                if (root.TryGetProperty("reference", out var reference) && reference.ValueKind == JsonValueKind.Object)
                {
                    gateway = GetString(reference, "gateway");
                    payment = GetString(reference, "payment");
                }

                string created;
                if (objectType == "refund")
                {
                    created = GetString(root, "created");
                }
                else if (root.TryGetProperty("transaction", out var transaction) && transaction.ValueKind == JsonValueKind.Object)
                {
                    created = GetString(transaction, "created");
                }
                else
                {
                    created = GetString(root, "created");
                }

                return "x_id" + id + "x_amount" + amount + "x_currency" + currency +
                       "x_gateway_reference" + gateway + "x_payment_reference" + payment +
                       "x_status" + status + "x_created" + created;
            }

            return null;
        }

        /// <summary>
        /// Formats the amount using the currency's ISO decimals
        /// (3 for BHD/KWD/OMR/JOD, 2 for the rest).
        /// </summary>
        public static string FormatAmount(decimal amount, string currency)
        {
            var decimals = 2;
            if (!string.IsNullOrEmpty(currency))
            {
                foreach (var c in ThreeDecimalCurrencies)
                {
                    if (string.Equals(c, currency, StringComparison.OrdinalIgnoreCase))
                    {
                        decimals = 3;
                        break;
                    }
                }
            }
            return amount.ToString("F" + decimals, CultureInfo.InvariantCulture);
        }

        public static string ComputeHmacSha256Hex(string key, string data)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                return sb.ToString();
            }
        }

        private static bool ConstantTimeEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private static string GetString(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.String) return prop.GetString() ?? string.Empty;
                if (prop.ValueKind == JsonValueKind.Number) return prop.GetRawText();
                if (prop.ValueKind == JsonValueKind.True) return "true";
                if (prop.ValueKind == JsonValueKind.False) return "false";
            }
            return string.Empty;
        }

        private static decimal GetDecimal(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.Number)
            {
                try { return prop.GetDecimal(); } catch { }
            }
            return 0m;
        }
    }
}
