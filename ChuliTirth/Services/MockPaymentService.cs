using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;

namespace ChuliTirth.Services;

// Development-mode stand-in for a real gateway. Settles every order instantly (no interactive
// checkout) so the booking flow works end-to-end without a configured gateway. Selected in
// Program.cs when Razorpay isn't configured or ApplicationSettings.PaymentEnabled is false.
public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;

    public MockPaymentService(ILogger<MockPaymentService> logger) => _logger = logger;

    public Task<PaymentOrderResult> CreateOrderAsync(Booking booking, decimal amount)
    {
        var orderId = $"MOCK-ORDER-{Guid.NewGuid():N}"[..24];
        _logger.LogInformation("Simulated payment order {OrderId} for {Amount} on booking {BookingNumber} — no real charge.", orderId, amount, booking.BookingNumber);

        return Task.FromResult(new PaymentOrderResult
        {
            Success = true,
            GatewayOrderId = orderId,
            AmountInSmallestUnit = (long)(amount * 100),
            Currency = "INR",
            RequiresCheckout = false,
            Message = "Payment simulated (development mode) — no real charge was made."
        });
    }

    public Task<PaymentResult> VerifyPaymentAsync(PaymentVerificationRequest request) =>
        Task.FromResult(new PaymentResult { Success = true, IsSimulated = true, TransactionRef = request.GatewayPaymentId, Message = "Simulated verification." });

    public Task<WebhookResult> ProcessWebhookAsync(string rawBody, string? signatureHeader) =>
        Task.FromResult(new WebhookResult { SignatureValid = true, Handled = false, Message = "Mock gateway does not send webhooks." });

    public Task<GatewayOrderStatus> GetOrderStatusAsync(string gatewayOrderId) =>
        Task.FromResult(new GatewayOrderStatus { Success = true, AnyPaymentCaptured = false, Message = "Mock gateway settles synchronously — there's nothing to reconcile." });

    public Task<PaymentResult> RefundAsync(Payment payment, decimal? amount = null)
    {
        var reference = $"REFUND-SIM-{Guid.NewGuid():N}"[..24];
        _logger.LogInformation("Simulated refund of {Amount} for payment {PaymentId} (ref {Reference})", amount?.ToString("F2") ?? "full amount", payment.Id, reference);
        return Task.FromResult(new PaymentResult { Success = true, TransactionRef = reference, IsSimulated = true, Message = "Refund simulated (development mode)." });
    }
}
