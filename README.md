# Chuli Tirth Dharamshala — Room Booking Website

A production-shaped ASP.NET Core MVC + EF Core + MySQL application for a Jain Tirth/Dharamshala:
room browsing, transaction-safe availability search and booking, a Jain Tithi calendar and
quotes ticker, multilingual (English/Gujarati/Hindi) content, and an Admin panel for rooms,
bookings, and site content.

Runs directly with `dotnet run` — no Docker, no containers.

## Solution layout

```
ChuliTirth.sln
ChuliTirth/            the web app (MVC)
ChuliTirth.Tests/       xUnit tests (booking + availability logic)
```

Inside `ChuliTirth/`:

```
Controllers/            public site controllers
Areas/Admin/            admin panel (controllers + views, area-routed at /Admin)
Models/Entities/        EF Core entities
Models/ViewModels/      page view models
Models/DTOs/            service-layer request/result types
Models/Config/          strongly-typed appsettings sections
Data/                   ApplicationDbContext, EF configuration, seed data
Services/               business logic (booking, availability, payments, notifications, content)
Interfaces/             service contracts (DI)
ViewComponents/         reusable, DB-backed partials (tithi/quote tickers, footer, announcements)
Middleware/             request logging
Migrations/             EF Core Code-First migrations
Views/                  Razor views (public site)
Areas/Admin/Views/      Razor views (admin panel)
wwwroot/                CSS, JS, images
```

## Requirements

- .NET SDK 9.0+
- MySQL Server 8.0+ (this was scaffolded against Pomelo's MySQL provider; MariaDB 10.6+ also works)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Configuration

`appsettings.json` / `appsettings.Development.json` hold non-secret app settings. The DB
connection string is deliberately **not** committed anywhere — both files have
`"ConnectionStrings": { "DefaultConnection": "" }` as a placeholder. Supply the real value via
`dotnet user-secrets` locally or environment variables in production; never hardcode credentials
(including `root`) into a committed appsettings file.

## Database setup

Create a MySQL user scoped to this app's own database rather than using `root` — `root` has
unrestricted access to the whole MySQL server (all databases, user management, file I/O), so a
leaked `root` credential is a much bigger blast radius than a leaked app credential:

```sql
CREATE DATABASE IF NOT EXISTS chulitirth_dev;
CREATE USER 'chulitirth_app'@'localhost' IDENTIFIED BY 'a-strong-generated-password';
-- DML for normal operation, plus schema privileges because Program.cs auto-applies EF Core
-- migrations on startup (Database.Migrate()) using this same connection — all scoped to this
-- one database only, nothing server-wide.
GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, INDEX, REFERENCES,
      CREATE TEMPORARY TABLES, LOCK TABLES, EXECUTE ON chulitirth_dev.* TO 'chulitirth_app'@'localhost';
FLUSH PRIVILEGES;
```

(For stricter production hardening, you can split this further — a separate, more-privileged
user that runs `dotnet ef database update` during deploy, and a DML-only runtime user for the
app day to day. The single scoped user above is the minimum bar and what local dev uses.)

Then point the app at it:

```bash
cd ChuliTirth
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=chulitirth_dev;User=chulitirth_app;Password=a-strong-generated-password;TreatTinyAsBoolean=true;"
dotnet ef database update
```

This applies the migrations and creates the schema. In production, set the equivalent
`ConnectionStrings__DefaultConnection` environment variable instead of user-secrets (which is a
local-dev-only mechanism, not read outside Development).

**Note:** the app also runs `Database.Migrate()` automatically on startup in `Program.cs`
as a development convenience, so `dotnet run` alone will create/update the schema if you
skip the manual step above. It is wrapped in a try/catch and logs a warning rather than
crashing if MySQL isn't reachable — you'll see empty pages until the DB is available.

## Run

```bash
cd ChuliTirth
dotnet run
```

Then open the URL shown in the console (typically `https://localhost:5001` or similar).

On first run against an empty database, `DataSeeder` (in `Data/Seed/DataSeeder.cs`) seeds:
- Sample room types (AC / Non-AC / Family / Dormitory) with rooms, amenities and placeholder images
- Sample facilities, Bhojanshala timings, booking rules, gallery categories
- ~45 days of sample Jain Tithi calendar entries and a handful of Jain quotes (EN/GU/HI)
- A sample announcement and default site settings
- Three dev users (see below)

**All seeded content is clearly sample/placeholder data** — replace it via the Admin panel
(or directly in the seed data / database) before using this for a real Dharamshala. Nothing
about Chuli Tirth specifically was invented; anything Chuli-Tirth-specific should be entered
by an administrator who has verified it.

## Admin setup / test credentials (development only)

Seeded via `appsettings.json` → `SeedAdmin` section and `DataSeeder`:

| Role    | Email                      | Password       |
|---------|-----------------------------|----------------|
| Admin   | admin@chulitirth.local      | Admin@12345    |
| Manager | manager@chulitirth.local    | Manager@12345  |
| Guest   | guest@chulitirth.local      | Guest@12345    |

Log in at `/Account/Login`, then visit `/Admin`. **Change these before any non-local
deployment** — edit `appsettings.json` → `SeedAdmin` before the first run against a fresh
database (the seeder only runs once, when the `Users` table is empty), or update the user's
password hash directly afterward.

Authentication here is a **lightweight cookie-based auth built for development/testing**
(see `Services/AuthService.cs`), not ASP.NET Core Identity — this was an explicit choice
for this build to keep things simple while testing. It reuses Identity's
`PasswordHasher<T>` for hashing only. Swap in full ASP.NET Core Identity later if you need
email confirmation, external logins, lockout policies, etc. — `IAuthService` is the seam.

## Payments (Razorpay, with a mock fallback)

`IPaymentService` has two implementations:

- `MockPaymentService` — settles every booking instantly with a simulated transaction ref. Used
  whenever Razorpay isn't configured, so the booking flow always works in a fresh checkout.
- `RazorpayPaymentService` (`Services/RazorpayPaymentService.cs`) — talks to the Razorpay REST API
  directly (Basic Auth, no SDK dependency) to create an order, verify the client-side checkout
  signature, verify webhook signatures, and issue refunds.

`Program.cs` picks between them at startup: Razorpay is used only when **both**
`ApplicationSettings:PaymentEnabled` is `true` **and** `Razorpay:KeyId` / `Razorpay:KeySecret` are
set; otherwise it falls back to the mock automatically.

**Configuration** (`appsettings.json` → `Razorpay` section):

```json
"Razorpay": { "KeyId": "", "KeySecret": "", "WebhookSecret": "" }
```

`KeyId` is public and fine in source control. `KeySecret` and `WebhookSecret` are credentials —
set them via `dotnet user-secrets` locally or environment variables in production, never commit
them:

```bash
cd ChuliTirth
dotnet user-secrets set "Razorpay:KeyId" "rzp_test_xxxxxxxx"
dotnet user-secrets set "Razorpay:KeySecret" "your_test_key_secret"
```

Then set `"ApplicationSettings": { "PaymentEnabled": true }` in `appsettings.Development.json` to
switch the booking flow over to real checkout.

**Flow**: `Confirm` creates the booking + a Razorpay order, then redirects to `/Booking/Pay/{bookingNumber}`,
which opens Razorpay's Checkout.js widget. On success, the client posts the payment id/order id/signature
to `/Booking/PaymentCallback` for verification. `/Booking/Webhook` is the server-to-server confirmation
path (configure it in the Razorpay Dashboard under Webhooks, pointing at
`https://<your-domain>/Booking/Webhook`, with the same secret as `Razorpay:WebhookSecret`) — it's the
authoritative source of truth in case the guest's browser closes before the client-side callback fires.
Testing webhooks against `localhost` requires a tunnel (e.g. `ngrok http 5245`).

## Notifications (mock)

`INotificationService` fans out to `MockEmailNotificationService` and
`MockWhatsAppNotificationService`, which just log (`Services/NotificationServices.cs`) instead
of sending real email/WhatsApp. No external credentials required for local development.

## Running tests

```bash
dotnet test ChuliTirth.Tests/ChuliTirth.Tests.csproj
```

Covers: booking date/capacity/rules validation, transaction-safe double-booking prevention,
room availability overlap logic (including cancelled bookings and blocked/maintenance rooms),
booking number generation, and localization fallback.

## Key configurable settings

`appsettings.json` → `ApplicationSettings`:

| Setting | Purpose |
|---|---|
| `DefaultCulture` / `SupportedCultures` | gu-IN / hi-IN / en-US, culture-cookie based |
| `Currency`, `CurrencySymbol` | Display only; all storage is decimal INR |
| `BookingEnabled` | (wire into a booking-disabled banner if needed) |
| `PaymentEnabled`, `PaymentBypassInDevelopment` | Toggle for when a real gateway is added |
| `EmailEnabled`, `WhatsAppEnabled` | Toggle for when real senders are added |
| `ExternalJainCalendarApiEnabled` | Toggle for when `IJainCalendarService` gets a real backing API |
| `DefaultCheckInTime` / `DefaultCheckOutTime` | Also editable live via Admin → Settings |
| `TimeZoneId` | IST is hardcoded for display via `Helpers/IndianTimeHelper.cs` regardless of host OS |

Content editable live via **Admin → Settings**: Dharamshala name, address, phone, email,
WhatsApp, Google Maps URL, check-in/out time, homepage hero text, footer text — stored in the
`SiteSettings` table (`ISiteSettingsService`).

## External integrations that can be enabled later

All are behind interfaces so a real implementation can be dropped in without touching
controllers or views:

- **Payments** — `IPaymentService` (Razorpay/Stripe/PayU/etc.)
- **Email** — `INotificationService` (SendGrid/SMTP/etc.)
- **WhatsApp** — `INotificationService` (WhatsApp Business API/Twilio/etc.)
- **Jain calendar** — `IJainCalendarService` (a verified external Panchang/Tithi API instead of the seeded sample table)
- **Full ASP.NET Core Identity** — `IAuthService` (for external logins, email confirmation, 2FA/OTP)

## Deployment

### Windows IIS

1. Install the [.NET 9 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0) on the server.
2. `dotnet publish ChuliTirth/ChuliTirth.csproj -c Release -o C:\inetpub\chulitirth`
3. Create an IIS site pointing at that folder, with an Application Pool set to **No Managed Code**
   (the bundle's ASP.NET Core Module handles the runtime).
4. Set the connection string and any secrets via `web.config` `<environmentVariables>` or via
   `appsettings.Production.json` on the server (don't commit production secrets to source control).
5. Bind HTTPS with a certificate (IIS bindings, or a reverse proxy in front).
6. Ensure the app pool identity has network access to the MySQL server.

### Linux (systemd + Nginx)

1. Install the .NET 9 runtime: `sudo apt-get install dotnet-runtime-9.0` (or your distro's equivalent).
2. `dotnet publish ChuliTirth/ChuliTirth.csproj -c Release -o /var/www/chulitirth`
3. Create a systemd unit, e.g. `/etc/systemd/system/chulitirth.service`:
   ```ini
   [Unit]
   Description=Chuli Tirth Dharamshala
   After=network.target

   [Service]
   WorkingDirectory=/var/www/chulitirth
   ExecStart=/usr/bin/dotnet /var/www/chulitirth/ChuliTirth.dll
   Restart=always
   RestartSec=10
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5000
   Environment=ConnectionStrings__DefaultConnection=Server=localhost;Database=chulitirth;User=chulitirth;Password=CHANGE_ME;

   [Install]
   WantedBy=multi-user.target
   ```
4. `sudo systemctl enable --now chulitirth`
5. Reverse-proxy with Nginx (forward `Host`, `X-Forwarded-For`, `X-Forwarded-Proto`) and terminate
   HTTPS there (e.g. via Let's Encrypt/certbot).
6. Point MySQL at a dedicated `chulitirth` database/user rather than `root`.

## Known simplifications (by design, for this dev/testing build)

- **Auth** is lightweight cookie auth, not full ASP.NET Core Identity — an explicit choice made
  for this build. It does include failed-login lockout (5 attempts → 15 minute lockout) and a
  token-based password reset flow (`AuthService` + `/Account/ForgotPassword` /
  `/Account/ResetPassword`), which is a meaningful chunk of what a public booking site handling
  PII + payments needs. Still missing versus full Identity: email verification, 2FA, external
  logins. `IAuthMailer`/`MockAuthMailer` log the reset link instead of emailing it — swap that
  registration in `Program.cs` for a real mailer when ready, same pattern as `INotificationService`.
- **Payments**: Razorpay is wired up behind `IPaymentService` (see the Payments section above),
  falling back to a mock when not configured.
- **Email/WhatsApp** notifications (booking events) are logged, not sent — same "swap the DI
  registration" pattern as the auth mailer above.
- **Static UI chrome** (nav, buttons, common field labels, table headers) is localized via
  `Helpers/UiText.cs` — a small static EN/GU/HI dictionary, not a full `.resx` resource setup.
  It covers the chrome a guest actually navigates through; longer descriptive/sample paragraph
  text and `[Display(Name=...)]`-generated form labels are still English-only. All **DB content**
  (room descriptions, facilities, Tithi, quotes, announcements, booking rules) is fully trilingual
  via `LocalizationHelper`. Moving the remaining chrome to proper `.resx` + `IStringLocalizer` is
  still reasonable follow-up work if you need every string translatable without a code change.
- **Jain Tithi calendar**: there is no reliable free/public Jain Panchang API to integrate against,
  so `IJainCalendarService` reads from the `JainTithis` table, which Admin → Jain Tithi (full
  Create/Edit/Delete) manages directly. Seeded data is clearly-marked sample data for demo
  purposes only — before going live, replace it with dates verified against your own Panchang
  reference. This (admin-entered, trust-verified data) is the intended production path, not a
  placeholder waiting on an external API.
- Gallery uses a simple new-tab image view rather than a JS lightbox library.

## Production checklist (security-relevant items addressed so far)

- [x] DB credentials out of source control — `ConnectionStrings:DefaultConnection` is empty in
      every committed appsettings file; set it via `dotnet user-secrets` (dev) or the
      `ConnectionStrings__DefaultConnection` env var (prod).
- [x] App runs as a MySQL user scoped to its own database, not `root` (see Database setup above).
- [x] Demo Manager/Guest accounts (with publicly-documented passwords) only seed in Development —
      a production run seeds only the configured `SeedAdmin` account.
- [x] A startup warning logs if running outside Development with the default `SeedAdmin` email
      still set — rotate `SeedAdmin:Email`/`SeedAdmin:Password` before the first production run.
- [x] Failed-login lockout and password reset (see Auth above).
- [ ] Full ASP.NET Core Identity (email verification, 2FA) — only if you need it.
- [ ] Real email/WhatsApp sending — currently logged only.
- [ ] HTTPS/domain/reverse-proxy setup — see Deployment above; not done by this repo.
- [ ] Rate limiting on the booking and contact forms.
- [ ] Centralized log aggregation / uptime monitoring beyond local Serilog files.
