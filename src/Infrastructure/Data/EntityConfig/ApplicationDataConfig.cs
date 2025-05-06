using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

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

            builder.Property(ad => ad.AreaId)
                .IsRequired();

            builder.HasOne(r => r.Area)
                .WithMany(a => a.Applications)
                .HasForeignKey(r => r.AreaId)
                .IsRequired();
        }
    }
}
