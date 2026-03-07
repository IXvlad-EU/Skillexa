using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Jobs)
            .WithOne(j => j.User)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
