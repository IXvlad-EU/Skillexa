using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedOnAdd();

        builder.Property(rt => rt.TokenHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.HasIndex(rt => rt.UserId);

        builder.HasIndex(rt => rt.TokenHash);
    }
}
