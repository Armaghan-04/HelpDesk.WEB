namespace HelpDesk.WEB.Models.Entities
{
    public static class Roles
    {
        public const string Employee = nameof(Employee);
        public const string Technician = nameof(Technician);
        public const string Manager = nameof(Manager);

        public static readonly string[] All = [Employee, Technician, Manager];
    }
}
