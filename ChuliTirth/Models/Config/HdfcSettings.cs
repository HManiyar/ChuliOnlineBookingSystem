namespace ChuliTirth.Models.Config;

// Placeholder shape for HDFC SmartHub (SmartGATEWAY) credentials — field names are a best guess
// based on common Indian bank-PG patterns (merchant id + working key + a response-signature
// secret) and WILL need adjusting once HDFC's relationship manager hands over the actual
// integration kit. See HDFC_INTEGRATION.md at the repo root for what to do with that kit.
public class HdfcSettings
{
    public const string SectionName = "Hdfc";

    public string MerchantId { get; set; } = string.Empty;
    public string WorkingKey { get; set; } = string.Empty;
    public string ResponseSignatureSecret { get; set; } = string.Empty;

    // Base URL for HDFC's API/redirect endpoints — HDFC will supply separate sandbox and
    // production URLs; do not guess these, paste them in exactly as given.
    public string ApiBaseUrl { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(MerchantId) &&
        !string.IsNullOrWhiteSpace(WorkingKey) &&
        !string.IsNullOrWhiteSpace(ApiBaseUrl);
}
