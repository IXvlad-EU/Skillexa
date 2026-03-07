using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skillexa.Engine.Domain;

namespace Skillexa.Engine.Data.Configurations;

public sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.TemplateKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Version)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .IsRequired();

        builder.Property(t => t.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasDefaultValueSql("now() at time zone 'utc'")
            .IsRequired();
    }
}
