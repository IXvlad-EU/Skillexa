using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class DocumentStatusConfiguration : IEntityTypeConfiguration<DocumentStatus>
{
    public void Configure(EntityTypeBuilder<DocumentStatus> builder)
    {
        builder.HasKey(ds => ds.Id);

        builder.Property(ds => ds.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ds => ds.Name)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(ds => ds.Name)
            .IsUnique();

        builder.HasData(
            new DocumentStatus { Id = 1, Name = "Queued" },
            new DocumentStatus { Id = 2, Name = "Processing" },
            new DocumentStatus { Id = 3, Name = "Succeeded" },
            new DocumentStatus { Id = 4, Name = "Failed" });
    }
}
