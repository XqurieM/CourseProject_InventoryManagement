namespace CourseProject_InventoryManagement.Infrastructure.Keys
{
    public class SalesforceSettings
    {
        public const string SectionName = "Salesforce";

        public string AuthUrl { get; set; } = "https://login.salesforce.com/services/oauth2/token";
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SecurityToken { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }
}
