using HelpDesk.WEB.Common.Results;

namespace HelpDesk.WEB.Common.Errors
{
    /// <summary>
    /// Expected failures from the manager-only department management rules.
    /// </summary>
    public static class DepartmentErrors
    {
        public static readonly Error NotFound = new(
            "Department.NotFound",
            "Department not found.");

        public static readonly Error LastActiveDepartment = new(
            "Department.LastActiveDepartment",
            "At least one active department must remain. Activate another department before deactivating this one.");

        /// <summary>
        /// Duplicate name detected by the service pre-check, including the submitted name.
        /// </summary>
        public static Error DuplicateName(string name) => new(
            "Department.DuplicateName",
            $"A department named '{name}' already exists.");

        /// <summary>
        /// Duplicate name detected by the unique index after the pre-check raced another request.
        /// </summary>
        public static readonly Error DuplicateNameConflict = new(
            "Department.DuplicateName",
            "A department with that name already exists.");
    }
}
