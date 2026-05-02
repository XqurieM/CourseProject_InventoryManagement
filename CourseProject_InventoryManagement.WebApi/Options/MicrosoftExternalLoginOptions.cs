namespace CourseProject_InventoryManagement.WebApi.Options;

public sealed class MicrosoftExternalLoginOptions
{
    public const string SectionName = "MicrosoftExternalLogin";

    public string ClientId { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ObjectId { get; set; } = string.Empty;
}
