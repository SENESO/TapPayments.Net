using System;
using System.Net;

namespace TapPayments
{
    /// <summary>
    /// Thrown when the Tap API returns an error response.
    /// </summary>
    public class TapApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        /// <summary>The Tap error code, if the API provided one.</summary>
        public string ErrorCode { get; }

        /// <summary>The Tap error description, if the API provided one.</summary>
        public string ErrorDescription { get; }

        public TapApiException(string message, HttpStatusCode statusCode, string errorCode = null, string errorDescription = null, Exception inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            ErrorDescription = errorDescription;
        }
    }
}
