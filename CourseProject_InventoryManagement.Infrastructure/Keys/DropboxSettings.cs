namespace CourseProject_InventoryManagement.Infrastructure.Keys
{
    public class DropboxSettings
    {
        public const string SectionName = "DropboxSettings";

        public string AccessToken { get; set; } = string.Empty;
        public string FolderPath { get; set; } = "/support_tickets";
        public string AppKey { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
