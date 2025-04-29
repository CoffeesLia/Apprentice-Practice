using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    internal class IntegrationConfig : IEntityTypeConfiguration<Integration>
    {
        public void Configure(EntityTypeBuilder<Integration> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Integration");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name);
            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(255);
            builder.HasOne(p => p.ApplicationData)
                .WithMany(a => a.Integration)
                .HasForeignKey(p => p.Id);
        }
    }
}
