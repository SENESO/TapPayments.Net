using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TapPayments;
using TapPayments.Models;

namespace TapPayments.IntegrationTests
{
    /// <summary>
    /// Live tests against Tap's real API (https://api.tap.company/v2).
    ///
    /// These tests only create INITIATED charges/customers — they never submit
    /// card details or complete a payment, so no real charge can occur.
    ///
    /// Credentials come from environment variables. Every test calls
    /// Assert.Ignore() naming the missing variable when it is absent, so the
    /// suite stays green on machines without test credentials.
    ///
    /// Required: TAP_SECRET_KEY (sk_test_... from the Tap dashboard).
    /// </summary>
    [Category("Integration")]
    public class TapIntegrationTests
    {
        private const string SecretKeyVar = "TAP_SECRET_KEY";

        private TapClient Client()
        {
            var key = Environment.GetEnvironmentVariable(SecretKeyVar);
            if (string.IsNullOrWhiteSpace(key))
                Assert.Ignore($"Set {SecretKeyVar} to run integration tests.");
            return new TapClient(key);
        }

        [Test]
        public async Task CreateCharge_AgainstLiveApi_ReturnsInitiatedCharge()
        {
            var client = Client();

            var charge = await client.CreateChargeAsync(new CreateChargeRequest
            {
                Amount = 1,
                Currency = "SAR",
                Description = "TapPayments.Net integration test",
                Customer = new TapCustomer
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    Phone = new TapPhone { CountryCode = "966", Number = "500000001" }
                },
                Source = new TapSource { Id = TapSources.All },
                Redirect = new TapRedirect { Url = "https://example.com/redirect" }
            });

            Assert.That(charge.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(charge.Status, Is.EqualTo(TapChargeStatus.Initiated));
        }

        [Test]
        public async Task CreateCustomer_AgainstLiveApi_ReturnsCustomerId()
        {
            var client = Client();

            var customer = await client.CreateCustomerAsync(new CreateCustomerRequest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            });

            Assert.That(customer.Id, Is.Not.Null.And.Not.Empty);
        }
    }
}
