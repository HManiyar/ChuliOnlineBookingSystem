using ChuliTirth.Interfaces;
using ChuliTirth.Models.Config;
using ChuliTirth.Models.Entities;
using Microsoft.Extensions.Options;

namespace ChuliTirth.Services;

// SCAFFOLD — not a working integration yet. Mirrors RazorpayPaymentService's shape so switching
// the active gateway later is a one-line change in Program.cs, but every method below is a
// deliberate placeholder: HDFC SmartHub's API (endpoints, auth, request/response shapes, and the
// signature/encryption scheme used to verify a payment) is not public documentation — it's only
// handed over after the Trust completes onboarding with its HDFC relationship manager. Rather
// than invent plausible-looking endpoint URLs or a signature algorithm that might be wrong (and
// silently insecure), every gateway-specific detail here is a clearly marked TODO.
//
// See HDFC_INTEGRATION.md at the repo root for what to do once that integration kit arrives.
public class HdfcPaymentService : IPaymentService
{
    private readonly HttpClient _http;
    private readonly HdfcSettings _settings;
    private readonly ILogger<HdfcPaymentService> _logger;

    public HdfcPaymentService(HttpClient http, IOptions<HdfcSettings> settings, ILogger<HdfcPaymentService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<PaymentOrderResult> CreateOrderAsync(Booking booking, decimal amount)
    {
        // TODO once HDFC's integration kit arrives:
        //   1. Confirm the order-creation endpoint under _settings.ApiBaseUrl (HDFC will supply
        //      separate sandbox/UAT and production base URLs).
        //   2. Confirm the auth scheme — likely MerchantId + WorkingKey in the request, not a
        //      bearer token like Razorpay.
        //   3. Confirm the amount unit (Razorpay uses paise; confirm HDFC does the same before
        //      reusing that conversion).
        //   4. HDFC gateways are often redirect-based (the guest is POSTed to an HDFC-hosted
        //      page, not an embedded JS widget like Razorpay Checkout) — if so, PayViewModel and
        //      Pay.cshtml will need a form-POST redirect instead of the Razorpay Checkout.js call,
        //      and KeyId in PaymentOrderResult would carry whatever token that redirect form needs.
        _logger.LogWarning("HdfcPaymentService.CreateOrderAsync called but HDFC integration is not yet implemented.");
        return Task.FromResult(new PaymentOrderResult
        {
            Success = false,
            Message = "HDFC payment gateway integration is not yet implemented. See HDFC_INTEGRATION.md."
        });
    }

    public Task<PaymentResult> VerifyPaymentAsync(PaymentVerificationRequest request)
    {
        // TODO: HDFC's return/callback verification scheme needs confirming from the integration
        // kit — common patterns for Indian bank gateways are an HMAC-SHA256 signature over
        // specific response fields (like Razorpay's order_id|payment_id approach) or an
        // AES-encrypted response blob that must be decrypted with the working key first. Do not
        // assume either without checking the actual docs — an incorrect verification scheme is a
        // security hole, not just a bug.
        _logger.LogWarning("HdfcPaymentService.VerifyPaymentAsync called but HDFC integration is not yet implemented.");
        return Task.FromResult(new PaymentResult
        {
            Success = false,
            Message = "HDFC payment gateway integration is not yet implemented. See HDFC_INTEGRATION.md."
        });
    }

    public Task<WebhookResult> ProcessWebhookAsync(string rawBody, string? signatureHeader)
    {
        // TODO: confirm whether HDFC SmartHub sends server-to-server webhooks at all (some bank
        // gateways rely solely on the redirect callback plus a manual/API status check instead).
        // If it does, confirm the webhook payload shape and signature header name.
        _logger.LogWarning("HdfcPaymentService.ProcessWebhookAsync called but HDFC integration is not yet implemented.");
        return Task.FromResult(new WebhookResult
        {
            SignatureValid = false,
            Handled = false,
            Message = "HDFC payment gateway integration is not yet implemented. See HDFC_INTEGRATION.md."
        });
    }

    public Task<GatewayOrderStatus> GetOrderStatusAsync(string gatewayOrderId)
    {
        // TODO: confirm the order/transaction status-check endpoint — used by Admin's
        // "Verify Payment with Gateway" reconciliation action (see BookingService).
        _logger.LogWarning("HdfcPaymentService.GetOrderStatusAsync called but HDFC integration is not yet implemented.");
        return Task.FromResult(new GatewayOrderStatus
        {
            Success = false,
            Message = "HDFC payment gateway integration is not yet implemented. See HDFC_INTEGRATION.md."
        });
    }

    public Task<PaymentResult> RefundAsync(Payment payment, decimal? amount = null)
    {
        // TODO: confirm the refund endpoint and whether partial refunds (used by the
        // cancellation-policy logic in BookingService.CancelBookingAsync) are supported the same
        // way Razorpay's are — pass `amount` through once confirmed; omitting it should mean a
        // full refund, matching the IPaymentService contract.
        _logger.LogWarning("HdfcPaymentService.RefundAsync called but HDFC integration is not yet implemented.");
        return Task.FromResult(new PaymentResult
        {
            Success = false,
            Message = "HDFC payment gateway integration is not yet implemented. See HDFC_INTEGRATION.md."
        });
    }
}
