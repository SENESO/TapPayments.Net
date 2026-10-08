using System;

namespace TapPayments
{
    /// <summary>
    /// Options for <see cref="TapClient"/>.
    /// </summary>
    public class TapClientOptions
    {
        /// <summary>
        /// Your Tap secret API key (sk_test_... or sk_live_...). Required.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// API base URL. Defaults to https://api.tap.company/v2.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.tap.company/v2";

        /// <summary>
        /// Request timeout in seconds. Defaults to 30.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// When true, throws <see cref="TapApiException"/> for missing configuration.
        /// </summary>
        public bool ThrowOnMissingConfig { get; set; } = true;

        internal void Validate()
        {
            if (ThrowOnMissingConfig && string.IsNullOrWhiteSpace(SecretKey))
                throw new InvalidOperationException("TapClientOptions.SecretKey is required (sk_test_... or sk_live_...).");
        }
    }
}
