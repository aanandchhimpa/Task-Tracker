namespace TaskManagement.Api.Helpers
{
    public class JwtSettings
    {
        public string Secret { get; set; } = "REPLACE_WITH_STRONG_SECRET";
        public string Issuer { get; set; } = "TaskManagement";
        public string Audience { get; set; } = "TaskManagementClients";
        public int ExpiresMinutes { get; set; } = 60 * 24; // 24 hours
    }
}