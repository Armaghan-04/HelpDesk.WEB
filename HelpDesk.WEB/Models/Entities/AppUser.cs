using Microsoft.AspNetCore.Identity;

namespace HelpDesk.WEB.Models.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; private set; } = string.Empty;
        public Guid? DepartmentId { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAtUtc { get; private set; }

        public Department? Department { get; private set; }

        private AppUser() { }

        public AppUser(string fullName, string email, Guid? departmentId, DateTime utcNow)
        {
            Id = Guid.NewGuid();
            SetDetails(fullName, departmentId);

            ArgumentException.ThrowIfNullOrWhiteSpace(email);

            Email = email.Trim();
            UserName = Email;
            CreatedAtUtc = utcNow;
        }

        private void SetDetails(string fullName, Guid? departmentId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

            FullName = fullName.Trim();
            DepartmentId = departmentId;
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;
    }
}
