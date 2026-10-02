using ChuliTirth.Data;
using ChuliTirth.Data.Seed;
using ChuliTirth.Interfaces;
using ChuliTirth.Middleware;
using ChuliTirth.Models.Config;
using ChuliTirth.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/chulitirth-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();
builder.Host.UseSerilog();

// --- Configuration ---
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection(ApplicationSettings.SectionName));
builder.Services.Configure<SeedAdminSettings>(builder.Configuration.GetSection(SeedAdminSettings.SectionName));
builder.Services.Configure<RazorpaySettings>(builder.Configuration.GetSection(RazorpaySettings.SectionName));
builder.Services.Configure<HdfcSettings>(builder.Configuration.GetSection(HdfcSettings.SectionName));

// --- Database ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Fixed server version (rather than ServerVersion.AutoDetect) so app startup doesn't require a live DB connection.
var mySqlServerVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, mySqlServerVersion,
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(3)));

// --- MVC + Localization ---
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// --- Auth (lightweight cookie auth for dev/testing — see Services/AuthService.cs) ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Cookie.Name = "ChuliTirth.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorization();

// --- Application services ---
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomAvailabilityService, RoomAvailabilityService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Real gateway only when explicitly enabled AND the selected provider is fully configured —
// missing/partial config, or an unrecognized PaymentGatewayProvider value, falls back to the
// mock so the app never silently breaks in dev/staging.
var razorpaySettings = builder.Configuration.GetSection(RazorpaySettings.SectionName).Get<RazorpaySettings>() ?? new RazorpaySettings();
var hdfcSettings = builder.Configuration.GetSection(HdfcSettings.SectionName).Get<HdfcSettings>() ?? new HdfcSettings();
var appSettingsForPaymentSelection = builder.Configuration.GetSection(ApplicationSettings.SectionName).Get<ApplicationSettings>() ?? new ApplicationSettings();
var paymentEnabled = appSettingsForPaymentSelection.PaymentEnabled;
var gatewayProvider = appSettingsForPaymentSelection.PaymentGatewayProvider;

if (paymentEnabled && razorpaySettings.IsConfigured && gatewayProvider.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IPaymentService, RazorpayPaymentService>(client =>
    {
        client.BaseAddress = new Uri("https://api.razorpay.com/v1/");
        var authBytes = Encoding.ASCII.GetBytes($"{razorpaySettings.KeyId}:{razorpaySettings.KeySecret}");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    });
}
else if (paymentEnabled && hdfcSettings.IsConfigured && gatewayProvider.Equals("Hdfc", StringComparison.OrdinalIgnoreCase))
{
    // HdfcPaymentService is currently a scaffold (see Services/HdfcPaymentService.cs and
    // HDFC_INTEGRATION.md) — wiring it in here doesn't make checkout work yet, but means
    // completing that file is the only step left once HDFC's integration kit arrives.
    builder.Services.AddHttpClient<IPaymentService, HdfcPaymentService>(client =>
    {
        client.BaseAddress = new Uri(hdfcSettings.ApiBaseUrl);
    });
}
else
{
    builder.Services.AddScoped<IPaymentService, MockPaymentService>();
}
builder.Services.AddScoped<IJainCalendarService, JainCalendarService>();
builder.Services.AddScoped<IJainQuoteService, JainQuoteService>();
builder.Services.AddScoped<IGalleryService, GalleryService>();
builder.Services.AddScoped<IFacilityService, FacilityService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthMailer, MockAuthMailer>();

builder.Services.AddScoped<MockEmailNotificationService>();
builder.Services.AddScoped<MockWhatsAppNotificationService>();
builder.Services.AddScoped<INotificationService>(sp => new CompositeNotificationService(new INotificationService[]
{
    sp.GetRequiredService<MockEmailNotificationService>(),
    sp.GetRequiredService<MockWhatsAppNotificationService>()
}));

builder.Services.AddResponseCompression();

var app = builder.Build();

// --- Request culture (gu-IN / hi-IN / en-US) ---
var appSettings = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApplicationSettings>>().Value;
var supportedCultures = appSettings.SupportedCultures.Select(c => new System.Globalization.CultureInfo(c)).ToList();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(appSettings.DefaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    }
});

// --- Apply migrations + seed data on startup (dev convenience) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            db.Database.Migrate();
            await DataSeeder.SeedAsync(scope.ServiceProvider);
        }
        else
        {
            Log.Warning("No DefaultConnection configured — skipping migrations and seed data.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration/seed failed on startup.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCompression();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
