using NUnit.Framework;
using TapPayments.Webhooks;

namespace TapPayments.Tests
{
    public class WebhookValidatorTests
    {
        private const string SecretKey = "sk_test_fake";

        // Precomputed with the same canonical string + HMAC-SHA256 scheme.
        private const string ChargePayload = @"{
            ""id"": ""chg_TS04A0120261939e1a061b21"",
            ""amount"": 150,
            ""currency"": ""SAR"",
            ""status"": ""CAPTURED"",
            ""reference"": { ""gateway"": ""gw_1"", ""payment"": ""pay_1"" },
            ""transaction"": { ""created"": 1730000000000 }
        }";

        private static string HashOf(string canonical)
            => TapWebhookValidator.ComputeHmacSha256Hex(SecretKey, canonical);

        [Test]
        public void IsValid_AcceptsGenuineChargeWebhook()
        {
            var canonical = "x_idchg_TS04A0120261939e1a061b21" +
                            "x_amount150.00" +
                            "x_currencySAR" +
                            "x_gateway_referencegw_1" +
                            "x_payment_referencepay_1" +
                            "x_statusCAPTURED" +
                            "x_created1730000000000";

            Assert.That(TapWebhookValidator.IsValid(ChargePayload, HashOf(canonical), SecretKey, "charge"), Is.True);
        }

        [Test]
        public void IsValid_RejectsTamperedPayload()
        {
            var tampered = ChargePayload.Replace("CAPTURED", "VOID");
            var canonical = "x_idchg_TS04A0120261939e1a061b21x_amount150.00x_currencySARx_gateway_referencegw_1x_payment_referencepay_1x_statusCAPTUREDx_created1730000000000";

            Assert.That(TapWebhookValidator.IsValid(tampered, HashOf(canonical), SecretKey, "charge"), Is.False);
        }

        [Test]
        public void IsValid_RejectsWrongSecret()
        {
            var canonical = "x_idchg_TS04A0120261939e1a061b21x_amount150.00x_currencySARx_gateway_referencegw_1x_payment_referencepay_1x_statusCAPTUREDx_created1730000000000";

            Assert.That(TapWebhookValidator.IsValid(ChargePayload, HashOf(canonical), "sk_test_wrong", "charge"), Is.False);
        }

        [Test]
        public void IsValid_HandlesMissingReferencesAsEmpty()
        {
            var payload = @"{ ""id"": ""chg_1"", ""amount"": 10, ""currency"": ""AED"", ""status"": ""CAPTURED"", ""transaction"": { ""created"": 1000 } }";
            var canonical = "x_idchg_1x_amount10.00x_currencyAEDx_gateway_referencex_payment_referencex_statusCAPTUREDx_created1000";

            Assert.That(TapWebhookValidator.IsValid(payload, HashOf(canonical), SecretKey, "charge"), Is.True);
        }

        [Test]
        public void IsValid_ValidatesRefundWithTopLevelCreated()
        {
            var payload = @"{ ""id"": ""ref_1"", ""amount"": 25.5, ""currency"": ""KWD"", ""status"": ""REFUNDED"", ""created"": 2000 }";
            var canonical = "x_idref_1x_amount25.500x_currencyKWDx_gateway_referencex_payment_referencex_statusREFUNDEDx_created2000";

            Assert.That(TapWebhookValidator.IsValid(payload, HashOf(canonical), SecretKey, "refund"), Is.True);
        }

        [Test]
        public void IsValid_ValidatesInvoiceWithUpdatedField()
        {
            var payload = @"{ ""id"": ""inv_1"", ""amount"": 99.99, ""currency"": ""USD"", ""status"": ""PAID"", ""updated"": 3000, ""created"": 1000 }";
            var canonical = "x_idinv_1x_amount99.99x_currencyUSDx_updated3000x_statusPAIDx_created1000";

            Assert.That(TapWebhookValidator.IsValid(payload, HashOf(canonical), SecretKey, "invoice"), Is.True);
        }

        [Test]
        public void FormatAmount_UsesThreeDecimalsForKwd()
        {
            Assert.That(TapWebhookValidator.FormatAmount(25.5m, "KWD"), Is.EqualTo("25.500"));
            Assert.That(TapWebhookValidator.FormatAmount(150m, "SAR"), Is.EqualTo("150.00"));
            Assert.That(TapWebhookValidator.FormatAmount(150m, "EGP"), Is.EqualTo("150.00"));
            Assert.That(TapWebhookValidator.FormatAmount(1.234m, "BHD"), Is.EqualTo("1.234"));
        }

        [Test]
        public void IsValid_ReturnsFalseForGarbage()
        {
            Assert.That(TapWebhookValidator.IsValid("not json", "abc", SecretKey, "charge"), Is.False);
            Assert.That(TapWebhookValidator.IsValid(ChargePayload, null, SecretKey, "charge"), Is.False);
            Assert.That(TapWebhookValidator.IsValid(ChargePayload, "abc", SecretKey, "bogus"), Is.False);
        }
    }
}
