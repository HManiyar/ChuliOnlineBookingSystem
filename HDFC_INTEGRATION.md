# Completing the HDFC SmartHub Integration

This branch (`hdfc-integration`) adds the *scaffolding* for an HDFC SmartHub
(SmartGATEWAY) payment integration, mirroring the shape of the working
Razorpay integration on `main` so it's a contained fill-in-the-blanks job
once HDFC hands over their integration kit — not a rewrite.

## Why this isn't a working integration yet

Unlike Razorpay, HDFC SmartHub doesn't have public, self-service API
documentation. You only get the actual integration kit (API docs, sandbox
credentials, a merchant ID and working key) after the Trust's relationship
manager completes onboarding. Writing real request/response code against
guessed endpoint URLs or a guessed signature scheme would be worse than
useless here — a wrong signature-verification scheme is a security hole,
not just a bug, since it's exactly what stops someone from forging a fake
"payment succeeded" callback.

## What's already in place

- `Models/Config/HdfcSettings.cs` — credential shape (best-guess field
  names: `MerchantId`, `WorkingKey`, `ResponseSignatureSecret`,
  `ApiBaseUrl`). Adjust field names once you see HDFC's actual kit.
- `Services/HdfcPaymentService.cs` — implements `IPaymentService` (the same
  interface `RazorpayPaymentService` and `MockPaymentService` implement),
  so it's already wired into `BookingService`, the Admin "Verify Payment"
  reconciliation action, and the cancellation-refund flow. Every method
  currently returns a clear "not yet implemented" failure rather than
  throwing, so flipping the config switch on prematurely fails safely
  instead of 500ing.
- `Program.cs` — `ApplicationSettings.PaymentGatewayProvider` selects
  between `"Razorpay"` and `"Hdfc"` (falls back to the mock gateway if the
  selected provider isn't fully configured, so a typo can't silently break
  checkout in production).
- `appsettings.json` — empty `Hdfc` section placeholder. **`WorkingKey` and
  `ResponseSignatureSecret` are credentials — set them via
  `dotnet user-secrets` (dev) or environment variables (production), never
  commit them, exactly like `Razorpay:KeySecret` already works.**

## What to do once HDFC's integration kit arrives

1. **Read the kit's API reference first.** Confirm:
   - The actual base URL(s) for sandbox/UAT and production.
   - The auth scheme (likely `MerchantId` + `WorkingKey` in the request
     body/headers, not a Bearer token).
   - The order-creation request/response shape, and whether checkout is a
     **redirect** (guest is POSTed to an HDFC-hosted page) or an
     **embedded widget** like Razorpay Checkout.js. This changes how
     `Views/Booking/Pay.cshtml` needs to work — a redirect flow needs a
     plain auto-submitting form, not a JS widget call.
   - The payment verification scheme (HMAC signature over specific fields,
     or an encrypted response blob to decrypt) — implement this exactly as
     documented, don't adapt Razorpay's `order_id|payment_id` HMAC pattern
     without confirming HDFC uses the same shape.
   - Whether webhooks exist, and their payload/signature format.
   - The refund endpoint and whether it supports partial amounts (needed
     for the tiered cancellation-refund logic in
     `BookingService.CancelBookingAsync` / `Helpers/CancellationPolicy.cs`).

2. **Fill in `Services/HdfcPaymentService.cs` method by method** — each
   method has a `// TODO` comment block explaining exactly what it needs.
   Use `Services/RazorpayPaymentService.cs` as a structural reference for
   patterns (error handling, logging, HMAC verification), not for the
   specific endpoint/field names, which will differ.

3. **If checkout turns out to be redirect-based** (not an embedded widget),
   update `PayViewModel` (`Models/ViewModels/BookingViewModels.cs`) and
   `Views/Booking/Pay.cshtml` to render an auto-submitting HTML form posting
   to HDFC's hosted page instead of loading Checkout.js.

4. **Test against HDFC's sandbox/UAT environment** before touching
   production credentials — same approach used for Razorpay's test mode
   earlier in this project.

5. **Switch over**: set `ApplicationSettings:PaymentGatewayProvider` to
   `"Hdfc"` and `ApplicationSettings:PaymentEnabled` to `true`, with the
   `Hdfc:*` secrets set via user-secrets/environment variables. The rest of
   the booking flow (order creation → payment → verification → admin
   reconciliation → cancellation refunds) needs no changes — that's the
   point of going through `IPaymentService`.

## Testing checklist once implemented

Mirror what was already verified for Razorpay (see the project history /
`ChuliTirth.Tests` for the equivalent Razorpay tests):
- [ ] Order creation succeeds and is visible on HDFC's merchant dashboard
- [ ] A real sandbox payment completes and marks the booking Paid
- [ ] A tampered/forged verification request is correctly rejected
- [ ] `GetOrderStatusAsync` correctly reconciles a payment that succeeded on
      HDFC's side but whose callback/webhook never reached the app
- [ ] A full refund and a partial refund (for the 50%-tier cancellation
      case) both work
