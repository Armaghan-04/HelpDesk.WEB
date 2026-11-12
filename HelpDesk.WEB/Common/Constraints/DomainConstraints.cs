namespace HelpDesk.WEB.Common.Constraints
{
    /// <summary>
    /// Length limits that are enforced in more than one layer (validation, EF
    /// configuration and Razor views). Keeping them here stops the copies from
    /// silently drifting apart.
    /// </summary>
    public static class DomainConstraints
    {
        public static class Ticket
        {
            public const int TitleMinLength = 3;
            public const int TitleMaxLength = 200;

            public const int DescriptionMinLength = 10;
            public const int DescriptionMaxLength = 4000;
        }

        public static class TicketComment
        {
            public const int BodyMaxLength = 4000;
        }

        public static class Department
        {
            public const int NameMinLength = 2;
            public const int NameMaxLength = 150;

            public const int DescriptionMaxLength = 1000;
        }

        public static class User
        {
            public const int FullNameMinLength = 2;
            public const int FullNameMaxLength = 120;
        }
    }
}
