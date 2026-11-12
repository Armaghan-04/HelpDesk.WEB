using HelpDesk.WEB.Common.Results;

namespace HelpDesk.WEB.Common.Errors
{
    /// <summary>
    /// Expected failures from the manager-only user management rules.
    /// </summary>
    public static class UserErrors
    {
        public static readonly Error NotFound = new(
            "User.NotFound",
            "User not found.");

        public static readonly Error InvalidRole = new(
            "User.InvalidRole",
            "Choose a valid role.");

        public static readonly Error CannotChangeOwnRole = new(
            "User.CannotChangeOwnRole",
            "You can't change your own role. Ask another manager.");

        public static readonly Error CannotDeactivateSelf = new(
            "User.CannotDeactivateSelf",
            "You can't deactivate your own account. Ask another manager.");

        public static readonly Error LastActiveManagerRoleChange = new(
            "User.LastActiveManagerRoleChange",
            "At least one active manager must remain. Promote someone else first.");

        public static readonly Error LastActiveManagerDeactivation = new(
            "User.LastActiveManagerDeactivation",
            "At least one active manager must remain. Promote another manager before deactivating this one.");
    }
}
