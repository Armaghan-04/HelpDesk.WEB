using HelpDesk.WEB.Common.Results;

namespace HelpDesk.WEB.Common.Errors
{
    /// <summary>
    /// Expected failures from the ticket workflow rules.
    /// </summary>
    public static class TicketErrors
    {
        public static readonly Error NotFound = new(
            "Ticket.NotFound",
            "Ticket not found.");

        public static readonly Error UserNotFound = new(
            "Ticket.UserNotFound",
            "User not found.");

        public static readonly Error UserInactive = new(
            "Ticket.UserInactive",
            "Your account is inactive.");

        public static readonly Error DepartmentNotFound = new(
            "Ticket.DepartmentNotFound",
            "Department not found.");

        public static readonly Error TechnicianNotFound = new(
            "Ticket.TechnicianNotFound",
            "Technician not found.");

        public static readonly Error ForbiddenEdit = new(
            "Ticket.ForbiddenEdit",
            "You are not allowed to edit this ticket.");

        public static readonly Error ForbiddenComment = new(
            "Ticket.ForbiddenComment",
            "You are not allowed to comment on this ticket.");

        public static readonly Error ForbiddenAction = new(
            "Ticket.ForbiddenAction",
            "You are not allowed to perform this action.");

        public static readonly Error ConcurrencyConflict = new(
            "Ticket.ConcurrencyConflict",
            "This ticket was changed by another user. Reload it and try again.");

        /// <summary>
        /// Inactive department detected during ticket creation or editing,
        /// including the department name.
        /// </summary>
        public static Error DepartmentInactive(string name) => new(
            "Ticket.DepartmentInactive",
            $"Department '{name}' is inactive.");

        /// <summary>
        /// Inactive assignee detected during assignment, including the user's name.
        /// </summary>
        public static Error TechnicianInactive(string fullName) => new(
            "Ticket.TechnicianInactive",
            $"{fullName} is not an active user.");

        /// <summary>
        /// Assignee without the Technician or Manager role, including the user's name.
        /// </summary>
        public static Error TechnicianNotStaff(string fullName) => new(
            "Ticket.TechnicianNotStaff",
            $"{fullName} is not a technician or manager.");

        /// <summary>
        /// Ticket entity state-machine or comment invariant, carrying the
        /// existing user-facing message unchanged.
        /// </summary>
        public static Error RuleViolation(string message) => new(
            "Ticket.RuleViolation",
            message);
    }
}
