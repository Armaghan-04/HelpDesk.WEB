namespace HelpDesk.WEB.Configuration
{
    /// <summary>
    /// Configuration-driven switches for optional account features.
    /// Bound from the "Authentication" section so environments can enable or disable
    /// features without hardcoded environment checks.
    /// </summary>
    public sealed class AuthenticationSettings
    {
        public const string SectionName = "Authentication";

        public RegistrationSettings Registration { get; set; } = new();

        public PasswordRecoverySettings PasswordRecovery { get; set; } = new();

        public sealed class RegistrationSettings
        {
            public bool Enabled { get; set; }
        }

        public sealed class PasswordRecoverySettings
        {
            public bool Enabled { get; set; }
        }
    }
}
