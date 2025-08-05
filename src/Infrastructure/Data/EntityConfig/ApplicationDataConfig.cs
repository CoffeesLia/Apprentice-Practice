using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class ApplicationDataConfig : IEntityTypeConfiguration<ApplicationData>
    {
        public void Configure(EntityTypeBuilder<ApplicationData> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("ApplicationData");

            builder.HasKey(ad => ad.Id);

            builder.Property(ad => ad.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasOne(ad => ad.Area)
            .WithMany(a => a.Applications)
            .HasForeignKey(ad => ad.AreaId);

            builder.Property(ad => ad.AreaId)
                .IsRequired();

            builder.HasOne(r => r.Area)
                .WithMany(a => a.Applications)
                .HasForeignKey(r => r.AreaId)
                .IsRequired();

            builder.HasOne(ad => ad.Squads)
            .WithMany()
            .HasForeignKey(ad => ad.SquadId)
            .IsRequired(false); // Torna o vínculo opcional
  
        }
    }
}
