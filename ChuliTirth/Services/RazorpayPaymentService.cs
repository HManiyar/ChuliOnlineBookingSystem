using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.Config;
using ChuliTirth.Models.Entities;
using Microsoft.Extensions.Options;

namespace ChuliTirth.Services;

// Talks to Razorpay's REST API directly (Basic Auth with KeyId:KeySecret) rather than pulling in
// their SDK — the surface we need (create order, verify a checkout signature, verify a webhook
// signature, issue a refund) is a handful of well-documented HTTP calls and two HMAC checks.
// Docs: https://razorpay.com/docs/api/orders/ and https://razorpay.com/docs/webhooks/
public class RazorpayPaymentService : IPaymentService
{
    private readonly HttpClient _http;
    private readonly RazorpaySettings _settings;
    private readonly ILogger<RazorpayPaymentService> _logger;

    public RazorpayPaymentService(HttpClient http, IOptions<RazorpaySettings> settings, ILogger<RazorpayPaymentService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaymentOrderResult> CreateOrderAsync(Booking booking, decimal amount)
    {
        var amountInPaise = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

        var payload = new
        {
            amount = amountInPaise,
            currency = "INR",
            receipt = booking.BookingNumber,
            payment_capture = 1,
            notes = new { bookingNumber = booking.BookingNumber, guestEmail = booking.Email }
        };

        try
        {
            using var response = await _http.PostAsJsonAsync("orders", payload);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay order creation failed ({Status}): {Body}", response.StatusCode, body);
                return new PaymentOrderResult { Success = false, Message = "Could not start the payment. Please try again shortly." };
            }

            var order = JsonSerializer.Deserialize<RazorpayOrder>(body, JsonOpts)!;
            return new PaymentOrderResult
            {
                Success = true,
                GatewayOrderId = order.Id,
                KeyId = _settings.KeyId,
                AmountInSmallestUnit = amountInPaise,
                Currency = "INR",
                RequiresCheckout = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay order creation threw an exception.");
            return new PaymentOrderResult { Success = false, Message = "Could not reach the payment gateway. Please try again shortly." };
        }
    }

    public Task<PaymentResult> VerifyPaymentAsync(PaymentVerificationRequest request)
    {
        var expected = HmacHex($"{request.GatewayOrderId}|{request.GatewayPaymentId}", _settings.KeySecret);
        var valid = FixedTimeEquals(expected, request.Signature);

        return Task.FromResult(valid
            ? new PaymentResult { Success = true, TransactionRef = request.GatewayPaymentId, IsSimulated = false, Message = "Payment verified." }
            : new PaymentResult { Success = false, IsSimulated = false, Message = "Payment signature could not be verified." });
    }

    public Task<WebhookResult> ProcessWebhookAsync(string rawBody, string? signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader) || string.IsNullOrWhiteSpace(_settings.WebhookSecret))
        {
            return Task.FromResult(new WebhookResult { SignatureValid = false, Message = "Missing webhook signature or secret." });
        }

        var expected = HmacHex(rawBody, _settings.WebhookSecret);
        if (!FixedTimeEquals(expected, signatureHeader))
        {
            return Task.FromResult(new WebhookResult { SignatureValid = false, Message = "Webhook signature mismatch." });
        }

        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            var eventName = root.GetProperty("event").GetString() ?? "";

            if (!root.TryGetProperty("payload", out var payload) ||
                !payload.TryGetProperty("payment", out var paymentWrap) ||
                !paymentWrap.TryGetProperty("entity", out var entity))
            {
                return Task.FromResult(new WebhookResult { SignatureValid = true, Handled = false, Message = $"Unhandled webhook event shape: {eventName}" });
            }

            var orderId = entity.TryGetProperty("order_id", out var o) ? o.GetString() : null;
            var paymentId = entity.TryGetProperty("id", out var p) ? p.GetString() : null;
            var succeeded = eventName is "payment.captured" or "order.paid";
            var failed = eventName is "payment.failed";

            if (!succeeded && !failed)
            {
                return Task.FromResult(new WebhookResult { SignatureValid = true, Handled = false, GatewayOrderId = orderId, GatewayPaymentId = paymentId, Message = $"Ignored event: {eventName}" });
            }

            return Task.FromResult(new WebhookResult
            {
                SignatureValid = true,
                Handled = true,
                GatewayOrderId = orderId,
                GatewayPaymentId = paymentId,
                PaymentSucceeded = succeeded
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Could not parse Razorpay webhook payload.");
            return Task.FromResult(new WebhookResult { SignatureValid = true, Handled = false, Message = "Malformed webhook payload." });
        }
    }

    public async Task<GatewayOrderStatus> GetOrderStatusAsync(string gatewayOrderId)
    {
        try
        {
            using var response = await _http.GetAsync($"orders/{gatewayOrderId}/payments");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay order status check failed ({Status}): {Body}", response.StatusCode, body);
                return new GatewayOrderStatus { Success = false, Message = "Could not reach the payment gateway." };
            }

            using var doc = JsonDocument.Parse(body);
            var items = doc.RootElement.TryGetProperty("items", out var itemsEl) ? itemsEl : default;
            if (items.ValueKind != JsonValueKind.Array || items.GetArrayLength() == 0)
            {
                return new GatewayOrderStatus { Success = true, AnyPaymentCaptured = false, Message = "No payment attempts found for this order." };
            }

            // Prefer a captured payment if there is one; otherwise report the most recent attempt.
            JsonElement? captured = null;
            JsonElement latest = items[0];
            foreach (var item in items.EnumerateArray())
            {
                var status = item.TryGetProperty("status", out var s) ? s.GetString() : null;
                if (status == "captured") captured = item;
            }
            var chosen = captured ?? latest;

            return new GatewayOrderStatus
            {
                Success = true,
                AnyPaymentCaptured = captured is not null,
                LatestPaymentId = chosen.TryGetProperty("id", out var id) ? id.GetString() : null,
                LatestPaymentStatus = chosen.TryGetProperty("status", out var st) ? st.GetString() : null,
                AmountPaidInSmallestUnit = chosen.TryGetProperty("amount", out var amt) ? amt.GetInt64() : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay order status check threw an exception.");
            return new GatewayOrderStatus { Success = false, Message = "Could not reach the payment gateway." };
        }
    }

    public async Task<PaymentResult> RefundAsync(Payment payment, decimal? amount = null)
    {
        if (string.IsNullOrWhiteSpace(payment.TransactionRef))
        {
            return new PaymentResult { Success = false, Message = "No gateway payment id on file to refund." };
        }

        try
        {
            // Omitting "amount" refunds the full captured amount; Razorpay takes it in paise.
            object payload = amount.HasValue
                ? new { amount = (long)Math.Round(amount.Value * 100, MidpointRounding.AwayFromZero) }
                : new { };
            using var response = await _http.PostAsJsonAsync($"payments/{payment.TransactionRef}/refund", payload);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay refund failed ({Status}): {Body}", response.StatusCode, body);
                return new PaymentResult { Success = false, Message = "Refund could not be processed." };
            }

            var refund = JsonSerializer.Deserialize<RazorpayRefund>(body, JsonOpts)!;
            return new PaymentResult { Success = true, TransactionRef = refund.Id, IsSimulated = false, Message = "Refund initiated." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Razorpay refund threw an exception.");
            return new PaymentResult { Success = false, Message = "Could not reach the payment gateway to process the refund." };
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private static string HmacHex(string message, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToHexStringLower(hash);
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    private record RazorpayOrder([property: JsonPropertyName("id")] string Id);
    private record RazorpayRefund([property: JsonPropertyName("id")] string Id);
}
