using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TapPayments;
using TapPayments.Models;

namespace TapPayments.Tests
{
    /// <summary>
    /// A mock handler that replays queued JSON responses and records requests.
    /// </summary>
    internal sealed class SequencedHandler : HttpMessageHandler
    {
        private readonly Queue<string> _responses;
        public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();
        public List<string> RequestBodies { get; } = new List<string>();

        public SequencedHandler(IEnumerable<string> responses)
        {
            _responses = new Queue<string>(responses);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            RequestBodies.Add(request.Content == null ? null : await request.Content.ReadAsStringAsync());
            var json = _responses.Count > 0 ? _responses.Dequeue() : "{}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }

    public class TapClientTests
    {
        private static TapClientOptions Options() => new TapClientOptions { SecretKey = "sk_test_fake" };

        private static CreateChargeRequest ChargeRequest() => new CreateChargeRequest
        {
            Amount = 150,
            Currency = "SAR",
            Description = "Test order",
            Customer = new TapCustomer
            {
                FirstName = "Ahmed",
                LastName = "Ali",
                Email = "ahmed@example.com",
                Phone = new TapPhone { CountryCode = "966", Number = "500000001" }
            },
            Source = new TapSource { Id = TapSources.All },
            Redirect = new TapRedirect { Url = "https://example.com/redirect" },
            Post = new TapPost { Url = "https://example.com/webhook" }
        };

        private const string ChargeJson = @"{
            ""id"": ""chg_TS04A0120261939e1a061b21"",
            ""object"": ""charge"",
            ""live_mode"": false,
            ""status"": ""INITIATED"",
            ""amount"": 150,
            ""currency"": ""SAR"",
            ""threeDSecure"": true,
            ""customer"": { ""first_name"": ""Ahmed"", ""email"": ""ahmed@example.com"" },
            ""transaction"": { ""created"": 1730000000000, ""url"": ""https://tap.company/checkout/abc"" },
            ""redirect"": { ""url"": ""https://example.com/redirect"" },
            ""post"": { ""url"": ""https://example.com/webhook"" }
        }";

        [Test]
        public void Constructor_ThrowsWhenSecretKeyMissing()
        {
            Assert.Throws<InvalidOperationException>(() => new TapClient(new TapClientOptions()));
        }

        [Test]
        public void Constructor_SetsBearerAuthorizationHeader()
        {
            var http = new HttpClient(new SequencedHandler(new[] { ChargeJson }));
            var client = new TapClient(Options(), http);

            Assert.That(http.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(http.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("sk_test_fake"));
        }

        [Test]
        public async Task CreateChargeAsync_PostsToChargesAndDeserializes()
        {
            var handler = new SequencedHandler(new[] { ChargeJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            var charge = await client.CreateChargeAsync(ChargeRequest());

            Assert.That(charge.Id, Is.EqualTo("chg_TS04A0120261939e1a061b21"));
            Assert.That(charge.Status, Is.EqualTo(TapChargeStatus.Initiated));
            Assert.That(charge.Amount, Is.EqualTo(150m));
            Assert.That(charge.Currency, Is.EqualTo("SAR"));
            Assert.That(charge.IsPendingRedirect, Is.True);
            Assert.That(charge.IsCaptured, Is.False);

            Assert.That(handler.Requests.Count, Is.EqualTo(1));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/charges"));
            Assert.That(handler.Requests[0].Method, Is.EqualTo(HttpMethod.Post));

            using (var doc = JsonDocument.Parse(handler.RequestBodies[0]))
            {
                var root = doc.RootElement;
                Assert.That(root.GetProperty("amount").GetDecimal(), Is.EqualTo(150m));
                Assert.That(root.GetProperty("currency").GetString(), Is.EqualTo("SAR"));
                Assert.That(root.GetProperty("source").GetProperty("id").GetString(), Is.EqualTo("src_all"));
                Assert.That(root.GetProperty("redirect").GetProperty("url").GetString(), Is.EqualTo("https://example.com/redirect"));
            }
        }

        [Test]
        public async Task RetrieveChargeAsync_GetsById()
        {
            var handler = new SequencedHandler(new[] { ChargeJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            var charge = await client.RetrieveChargeAsync("chg_123");

            Assert.That(charge.Id, Is.EqualTo("chg_TS04A0120261939e1a061b21"));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/charges/chg_123"));
            Assert.That(handler.Requests[0].Method, Is.EqualTo(HttpMethod.Get));
        }

        [Test]
        public void RetrieveChargeAsync_ThrowsOnEmptyId()
        {
            var client = new TapClient(Options(), new HttpClient(new SequencedHandler(new string[0])));
            Assert.ThrowsAsync<ArgumentException>(() => client.RetrieveChargeAsync(""));
        }

        private const string AuthorizeJson = @"{
            ""id"": ""auth_TS04A0120261939e1a061b21"",
            ""object"": ""authorize"",
            ""status"": ""AUTHORIZED"",
            ""amount"": 500,
            ""currency"": ""SAR""
        }";

        [Test]
        public async Task CreateAuthorizationAsync_PostsToAuthorize()
        {
            var handler = new SequencedHandler(new[] { AuthorizeJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            var auth = await client.CreateAuthorizationAsync(new CreateAuthorizationRequest
            {
                Amount = 500,
                Currency = "SAR",
                Customer = new TapCustomer { FirstName = "Ahmed", Email = "a@b.com" },
                Source = new TapSource { Id = "tok_abc" },
                Auto = new TapAuto { Type = "VOID", Time = 48 }
            });

            Assert.That(auth.Id, Is.EqualTo("auth_TS04A0120261939e1a061b21"));
            Assert.That(auth.IsAuthorized, Is.True);
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/authorize"));

            using (var doc = JsonDocument.Parse(handler.RequestBodies[0]))
            {
                var auto = doc.RootElement.GetProperty("auto");
                Assert.That(auto.GetProperty("type").GetString(), Is.EqualTo("VOID"));
                Assert.That(auto.GetProperty("time").GetInt32(), Is.EqualTo(48));
            }
        }

        [Test]
        public async Task VoidAuthorizationAsync_PostsToVoidEndpoint()
        {
            var handler = new SequencedHandler(new[] { @"{ ""id"": ""auth_1"", ""status"": ""VOID"" }" });
            var client = new TapClient(Options(), new HttpClient(handler));

            var auth = await client.VoidAuthorizationAsync("auth_1");

            Assert.That(auth.Status, Is.EqualTo("VOID"));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/authorize/auth_1/void"));
        }

        [Test]
        public async Task CaptureAuthorizationAsync_CreatesChargeWithAuthorizeSource()
        {
            var handler = new SequencedHandler(new[] { ChargeJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            await client.CaptureAuthorizationAsync("auth_1", 500m, "SAR");

            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/charges"));
            using (var doc = JsonDocument.Parse(handler.RequestBodies[0]))
            {
                Assert.That(doc.RootElement.GetProperty("source").GetProperty("id").GetString(), Is.EqualTo("auth_1"));
                Assert.That(doc.RootElement.GetProperty("amount").GetDecimal(), Is.EqualTo(500m));
                Assert.That(doc.RootElement.GetProperty("currency").GetString(), Is.EqualTo("SAR"));
            }
        }

        private const string RefundJson = @"{
            ""id"": ""ref_TS04A0120261939e1a061b21"",
            ""object"": ""refund"",
            ""status"": ""REFUNDED"",
            ""amount"": 50,
            ""currency"": ""SAR"",
            ""charge_id"": ""chg_123"",
            ""created"": 1730000001000
        }";

        [Test]
        public async Task CreateRefundAsync_PostsToRefunds()
        {
            var handler = new SequencedHandler(new[] { RefundJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            var refund = await client.CreateRefundAsync(new CreateRefundRequest
            {
                ChargeId = "chg_123",
                Amount = 50,
                Currency = "SAR",
                Reason = "Customer requested"
            });

            Assert.That(refund.Id, Is.EqualTo("ref_TS04A0120261939e1a061b21"));
            Assert.That(refund.ChargeId, Is.EqualTo("chg_123"));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/refunds"));

            using (var doc = JsonDocument.Parse(handler.RequestBodies[0]))
            {
                Assert.That(doc.RootElement.GetProperty("charge_id").GetString(), Is.EqualTo("chg_123"));
            }
        }

        [Test]
        public async Task RetrieveRefundAsync_GetsById()
        {
            var handler = new SequencedHandler(new[] { RefundJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            var refund = await client.RetrieveRefundAsync("ref_1");

            Assert.That(refund.Id, Is.EqualTo("ref_TS04A0120261939e1a061b21"));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/refunds/ref_1"));
        }

        private const string CustomerJson = @"{
            ""id"": ""cus_TS04A0120261939e1a061b21"",
            ""object"": ""customer"",
            ""first_name"": ""Ahmed"",
            ""last_name"": ""Ali"",
            ""email"": ""ahmed@example.com""
        }";

        [Test]
        public async Task CreateCustomerAsync_PostsToCustomers()
        {
            var handler = new SequencedHandler(new[] { CustomerJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            var customer = await client.CreateCustomerAsync(new CreateCustomerRequest
            {
                FirstName = "Ahmed",
                LastName = "Ali",
                Email = "ahmed@example.com"
            });

            Assert.That(customer.Id, Is.EqualTo("cus_TS04A0120261939e1a061b21"));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/customers"));
        }

        private const string TokenJson = @"{ ""id"": ""tok_abc123"", ""object"": ""token"", ""type"": ""CARD"", ""used"": false }";

        [Test]
        public async Task CreateTokenAsync_PostsToTokens()
        {
            var handler = new SequencedHandler(new[] { TokenJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            var token = await client.CreateTokenAsync(new CreateTokenRequest
            {
                Card = new TapCardDetails { Number = "4111111111111111", ExpMonth = 12, ExpYear = 2028, Cvc = "123", Name = "Ahmed Ali" }
            });

            Assert.That(token.Id, Is.EqualTo("tok_abc123"));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/tokens"));
        }

        [Test]
        public async Task CreateTokenFromSavedCardAsync_SendsSavedCardBlock()
        {
            var handler = new SequencedHandler(new[] { TokenJson });
            var client = new TapClient(Options(), new HttpClient(handler));

            await client.CreateTokenFromSavedCardAsync(new CreateTokenFromSavedCardRequest
            {
                SavedCard = new TapSavedCard { CardId = "card_1", CustomerId = "cus_1" }
            });

            using (var doc = JsonDocument.Parse(handler.RequestBodies[0]))
            {
                Assert.That(doc.RootElement.GetProperty("saved_card").GetProperty("card_id").GetString(), Is.EqualTo("card_1"));
                Assert.That(doc.RootElement.GetProperty("saved_card").GetProperty("customer_id").GetString(), Is.EqualTo("cus_1"));
            }
        }

        [Test]
        public async Task ListChargesAsync_PostsToChargesList()
        {
            var handler = new SequencedHandler(new[] { @"{ ""object"": ""list"", ""data"": [], ""has_more"": false, ""total"": 0 }" });
            var client = new TapClient(Options(), new HttpClient(handler));

            var list = await client.ListChargesAsync(new ChargeListRequest { Status = "CAPTURED", Limit = 10 });

            Assert.That(list.Total, Is.EqualTo(0));
            Assert.That(handler.Requests[0].RequestUri.AbsolutePath, Does.EndWith("/v2/charges/list"));
        }

        [Test]
        public void ApiError_ThrowsTapApiExceptionWithDetails()
        {
            var handler = new ErrorHandler(@"{ ""errors"": [ { ""code"": ""1100"", ""description"": ""Invalid API key"" } ] }", HttpStatusCode.Unauthorized);
            var client = new TapClient(Options(), new HttpClient(handler));

            var ex = Assert.ThrowsAsync<TapApiException>(() => client.RetrieveChargeAsync("chg_1"));
            Assert.That(ex.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(ex.ErrorCode, Is.EqualTo("1100"));
            Assert.That(ex.ErrorDescription, Is.EqualTo("Invalid API key"));
        }

        private sealed class ErrorHandler : HttpMessageHandler
        {
            private readonly string _json;
            private readonly HttpStatusCode _status;
            public ErrorHandler(string json, HttpStatusCode status) { _json = json; _status = status; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_json, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
