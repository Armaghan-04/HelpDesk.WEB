using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Common.Display
{
    /// <summary>
    /// Presentation-only formatting shared by Razor views. Keeps short identifiers,
    /// labels, dates and CSS class mappings in one place.
    /// </summary>
    public static class DisplayFormat
    {
        private const string DateFormat = "dd MMM yyyy";

        private const string DateTimeFormat = "dd MMM yyyy, HH:mm";

        public static string ShortId(Guid id) => id.ToString("N")[..8].ToUpperInvariant();

        public static string StatusLabel(TicketStatus status)
            => status == TicketStatus.InProgress ? "In progress" : status.ToString();

        public static string StatusCssClass(TicketStatus status) => status switch
        {
            TicketStatus.Open => "is-open",
            TicketStatus.Assigned => "is-assigned",
            TicketStatus.InProgress => "is-inprogress",
            TicketStatus.Resolved => "is-resolved",
            TicketStatus.Closed => "is-closed",
            TicketStatus.Reopened => "is-reopened",
            _ => "is-closed"
        };

        public static string LocalDate(DateTime utc, string format = DateFormat)
            => utc.ToLocalTime().ToString(format);

        public static string LocalDateTime(DateTime utc) => LocalDate(utc, DateTimeFormat);

        public static string RoleCssClass(string? role) => role switch
        {
            Roles.Manager => "is-manager",
            Roles.Technician => "is-technician",
            Roles.Employee => "is-employee",
            _ => "is-none"
        };
    }
}
