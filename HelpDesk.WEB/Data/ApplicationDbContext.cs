using HelpDesk.WEB.Common.Constraints;
using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.WEB.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {}

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<TicketComment> TicketComments { get; set; }

        public DbSet<TicketHistory> TicketHistories { get; set; }

        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUser>(user =>
            {
                user.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(DomainConstraints.User.FullNameMaxLength);

                user.HasOne(u => u.Department)
                    .WithMany()
                    .HasForeignKey(u => u.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                // The application does not collect phone numbers or offer 2FA. These members
                // come from IdentityUser, so ignoring them keeps them out of the schema entirely.
                user.Ignore(u => u.PhoneNumber);
                user.Ignore(u => u.PhoneNumberConfirmed);
                user.Ignore(u => u.TwoFactorEnabled);
            });

            builder.Entity<Department>(department =>
            {
                department.Property(d => d.Name)
                    .IsRequired()
                    .HasMaxLength(DomainConstraints.Department.NameMaxLength);

                department.Property(d => d.Description)
                    .HasMaxLength(DomainConstraints.Department.DescriptionMaxLength);

                department.HasIndex(d => d.Name)
                    .IsUnique();
            });

            builder.Entity<Ticket>(ticket =>
            {
                ticket.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(DomainConstraints.Ticket.TitleMaxLength);

                ticket.Property(t => t.Description)
                    .IsRequired()
                    .HasMaxLength(DomainConstraints.Ticket.DescriptionMaxLength);

                ticket.Property(t => t.RowVersion)
                    .IsRowVersion();

                ticket.HasOne(t => t.Department)
                    .WithMany()
                    .HasForeignKey(t => t.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                ticket.HasOne(t => t.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(t => t.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                ticket.HasOne(t => t.AssignedTo)
                    .WithMany()
                    .HasForeignKey(t => t.AssignedToId)
                    .OnDelete(DeleteBehavior.Restrict);

                ticket.HasMany(t => t.Comments)
                    .WithOne(c => c.Ticket)
                    .HasForeignKey(c => c.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                ticket.Navigation(t => t.Comments)
                    .UsePropertyAccessMode(PropertyAccessMode.Field);
            });

            builder.Entity<TicketComment>(comment =>
            {
                comment.Property(c => c.Body)
                    .IsRequired()
                    .HasMaxLength(DomainConstraints.TicketComment.BodyMaxLength);

                comment.HasOne(c => c.Author)
                    .WithMany()
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TicketHistory>(history =>
            {
                history.HasOne(h => h.Ticket)
                    .WithMany()
                    .HasForeignKey(h => h.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                history.HasOne(h => h.ChangedBy)
                    .WithMany()
                    .HasForeignKey(h => h.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict);

                history.HasIndex(h => new { h.TicketId, h.ChangedAtUtc });
            });
        }
    }
}
