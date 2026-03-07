using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(om => om.Id);

        builder.Property(om => om.Id)
            .ValueGeneratedOnAdd();

        builder.Property(om => om.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(om => om.PayloadJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(om => om.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();
    }
}
