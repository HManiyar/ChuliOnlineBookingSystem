namespace ChuliTirth.Models.Entities;

public class Payment : BaseEntity
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Method { get; set; } = "Mock";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Gateway order id (e.g. Razorpay "order_xxx"), set when the order is created — used to
    // correlate the async checkout callback and webhook back to this Payment/Booking.
    public string? GatewayOrderId { get; set; }

    // Gateway payment id (e.g. Razorpay "pay_xxx"), set once the payment is actually captured.
    public string? TransactionRef { get; set; }
    public bool IsSimulated { get; set; } = true;

    // How much of Amount has actually been refunded — may be less than Amount for a partial
    // (policy-based cancellation) refund. Status moves to Refunded as soon as any refund lands,
    // regardless of whether it was full or partial; this field is what distinguishes the two.
    public decimal? RefundedAmount { get; set; }
}
