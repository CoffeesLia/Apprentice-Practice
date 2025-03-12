using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;


namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class ApplicationDataConfig : IEntityTypeConfiguration<ApplicationData>
    {
        public void Configure(EntityTypeBuilder<ApplicationData> builder)
        {
            builder.ToTable("ApplicationData");

            builder.HasKey(ad => ad.Id);

            builder.Property(ad => ad.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(ad => ad.AreaId)
                .IsRequired();

            builder.HasOne(ad => ad.Area)
                .WithMany(a => a.Applications)
                .HasForeignKey(ad => ad.AreaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}