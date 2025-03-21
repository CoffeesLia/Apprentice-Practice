using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.Domain.EntityConfig
{
    public class ResponsavelConfig : IEntityTypeConfiguration<Responsible>
    {
        public void Configure(EntityTypeBuilder<Responsible> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Responsible");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.Nome)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Area)
                .IsRequired()
                .HasMaxLength(100);

            
        }
    }
}