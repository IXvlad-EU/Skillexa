using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Engine.Domain;

namespace Skillexa.Engine.Data.Configurations;

public sealed class ProviderQuotaConfiguration : IEntityTypeConfiguration<ProviderQuota>
{
    public void Configure(EntityTypeBuilder<ProviderQuota> builder)
    {
        builder.HasKey(pq => pq.Id);

        builder.Property(pq => pq.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pq => pq.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pq => pq.DayKey)
            .IsRequired();

        builder.Property(pq => pq.Used)
            .IsRequired();

        builder.Property(pq => pq.Limit)
            .IsRequired();

        builder.Property(pq => pq.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.HasIndex(pq => new { pq.Provider, pq.DayKey })
            .IsUnique();
    }
}
