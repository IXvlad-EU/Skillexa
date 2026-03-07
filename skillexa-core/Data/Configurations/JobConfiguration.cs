using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .ValueGeneratedOnAdd();

        builder.Property(j => j.TemplateKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(j => j.TemplateVersion)
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(j => j.Payload)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();

        builder.Property(j => j.PdfStorageKey)
            .HasMaxLength(500);

        builder.Property(j => j.SnapshotStorageKey)
            .HasMaxLength(500);

        builder.Property(j => j.ErrorCode)
            .HasMaxLength(100);

        builder.Property(j => j.CorrelationId)
            .IsRequired();

        builder.Property(j => j.IdempotencyKey)
            .IsRequired();

        builder.HasIndex(j => j.IdempotencyKey)
            .IsUnique();

        builder.Property(j => j.CreatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();

        builder.HasIndex(j => new { j.UserId, j.StatusId });

        builder.HasIndex(j => j.CreatedAt);

        builder.HasOne(j => j.Status)
            .WithMany(js => js.Jobs)
            .HasForeignKey(j => j.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
