using Microsoft.EntityFrameworkCore;
using AuthenticationService.Models;

namespace AuthenticationService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserDepartment> UserDepartments { get; set; }
    public DbSet<Delegation> Delegations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Roles)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserDepartment (many-to-many)
        modelBuilder.Entity<UserDepartment>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.DepartmentId });
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserDepartments)
                  .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.UserDepartments)
                  .HasForeignKey(e => e.DepartmentId);
        });

        // Delegation configuration
        modelBuilder.Entity<Delegation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Delegator)
                  .WithMany()
                  .HasForeignKey(e => e.DelegatorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Delegatee)
                  .WithMany()
                  .HasForeignKey(e => e.DelegateeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

