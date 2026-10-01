using ChuliTirth.Models.Entities;

namespace ChuliTirth.Interfaces;

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionRef { get; set; }
    public string? Message { get; set; }
    public bool IsSimulated { get; set; }
}

// Returned by CreateOrderAsync. RequiresCheckout tells the caller whether the guest must be
// sent through an interactive checkout (a real gateway) or whether the order already settled
// synchronously (the mock, or any future "pay at Dharamshala" option).
public class PaymentOrderResult
{
    public bool Success { get; set; }
    public string? GatewayOrderId { get; set; }
    public string? KeyId { get; set; }
    public long AmountInSmallestUnit { get; set; }
    public string Currency { get; set; } = "INR";
    public bool RequiresCheckout { get; set; }
    public string? Message { get; set; }
}

public class PaymentVerificationRequest
{
    public string GatewayOrderId { get; set; } = string.Empty;
    public string GatewayPaymentId { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

// Result of a verified webhook event, used to update the Payment/Booking it refers to.
public class WebhookResult
{
    public bool SignatureValid { get; set; }
    public bool Handled { get; set; }
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public bool PaymentSucceeded { get; set; }
    public string? Message { get; set; }
}

// Ground truth pulled directly from the gateway — used to reconcile a booking whose local
// Payment row is stuck Pending/Failed because the checkout callback and the webhook both never
// reached us (closed browser, server restart mid-request, webhook not yet configured, etc.)
// even though the guest's money was actually captured.
public class GatewayOrderStatus
{
    public bool Success { get; set; }
    public bool AnyPaymentCaptured { get; set; }
    public string? LatestPaymentId { get; set; }
    public string? LatestPaymentStatus { get; set; }
    public long AmountPaidInSmallestUnit { get; set; }
    public string? Message { get; set; }
}

public interface IPaymentService
{
    // Opens an order for the given amount. For the mock/bypass path this settles immediately
    // (RequiresCheckout = false); for a real gateway it returns an order id for client-side checkout.
    Task<PaymentOrderResult> CreateOrderAsync(Booking booking, decimal amount);

    // Verifies the signature posted back by the client-side checkout after payment.
    Task<PaymentResult> VerifyPaymentAsync(PaymentVerificationRequest request);

    // Verifies and parses a server-to-server webhook call (the authoritative confirmation path —
    // covers the guest closing the browser before the client-side callback fires).
    Task<WebhookResult> ProcessWebhookAsync(string rawBody, string? signatureHeader);

    // Asks the gateway directly whether an order actually has a captured payment against it —
    // for reconciling a booking stuck Pending/Failed locally despite the guest having paid.
    Task<GatewayOrderStatus> GetOrderStatusAsync(string gatewayOrderId);

    // amount: null refunds the full captured amount; a value refunds only that much (e.g. a
    // policy-based partial cancellation refund).
    Task<PaymentResult> RefundAsync(Payment payment, decimal? amount = null);
}
