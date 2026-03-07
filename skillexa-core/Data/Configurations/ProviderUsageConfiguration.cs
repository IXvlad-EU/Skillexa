using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class ProviderUsageConfiguration : IEntityTypeConfiguration<ProviderUsage>
{
    public void Configure(EntityTypeBuilder<ProviderUsage> builder)
    {
        builder.HasKey(pu => pu.Id);

        builder.Property(pu => pu.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pu => pu.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pu => pu.PeriodKey)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(pu => pu.Used)
            .IsRequired();

        builder.Property(pu => pu.Remaining)
            .IsRequired();

        builder.Property(pu => pu.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.HasIndex(pu => new { pu.Provider, pu.PeriodKey })
            .IsUnique();
    }
}
