namespace HelpDesk.WEB.Models.Entities
{
    public class Department : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Department() { }

        public Department(string name, string? description)
        {
            SetDetails(name, description);
        }

        public void Update(string name, string? description) => SetDetails(name, description);

        private void SetDetails(string name, string? description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            Name = name.Trim();
            Description = description?.Trim();
        }

        public void Activate() => IsActive = true;

        public void Deactivate() => IsActive = false;
    }
}
