namespace SimpleGateway.Configuration
{
    public class UserManagementSettings
    {
        public bool AllowSelfRegistration { get; set; } = true;
        public bool RequireEmailVerification { get; set; } = false;
        public string DefaultRole { get; set; } = "User";
    }
}
