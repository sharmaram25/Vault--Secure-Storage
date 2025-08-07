using Microsoft.EntityFrameworkCore;
using Vault.Core.Models;

namespace Vault.Infrastructure.Data;

public class VaultDbContext : DbContext
{
    public VaultDbContext(DbContextOptions<VaultDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Secret> Secrets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.PasswordHash)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasIndex(e => e.Username)
                .IsUnique();
            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        // Secret configuration
        modelBuilder.Entity<Secret>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.EncryptedContent)
                .IsRequired();
            entity.Property(e => e.InitializationVector)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .IsRequired();
            entity.Property(e => e.UserId)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(e => e.Secrets)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
