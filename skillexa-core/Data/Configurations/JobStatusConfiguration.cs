using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Configurations;

public sealed class JobStatusConfiguration : IEntityTypeConfiguration<JobStatus>
{
    public void Configure(EntityTypeBuilder<JobStatus> builder)
    {
        builder.HasKey(js => js.Id);

        builder.Property(js => js.Id)
            .ValueGeneratedOnAdd();

        builder.Property(js => js.Name)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(js => js.Name)
            .IsUnique();

        builder.HasData(
            new JobStatus { Id = 1, Name = "Queued" },
            new JobStatus { Id = 2, Name = "Processing" },
            new JobStatus { Id = 3, Name = "Succeeded" },
            new JobStatus { Id = 4, Name = "Failed" });
    }
}
