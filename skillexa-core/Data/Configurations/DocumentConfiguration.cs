using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedOnAdd();

        builder.Property(d => d.TemplateKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.TemplateVersion)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(d => d.Payload)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();

        builder.Property(d => d.PdfStorageKey)
            .HasMaxLength(500);

        builder.Property(d => d.SnapshotStorageKey)
            .HasMaxLength(500);

        builder.Property(d => d.ErrorCode)
            .HasMaxLength(100);

        builder.Property(d => d.CorrelationId)
            .IsRequired();

        builder.Property(d => d.IdempotencyKey)
            .IsRequired();

        builder.HasIndex(d => d.IdempotencyKey)
            .IsUnique();

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.HasIndex(d => new { d.UserId, d.StatusId });

        builder.HasIndex(d => d.CreatedAt);

        builder.HasOne(d => d.Status)
            .WithMany(ds => ds.Documents)
            .HasForeignKey(d => d.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
