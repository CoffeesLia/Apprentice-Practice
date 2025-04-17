using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class ServiceDataConfig : IEntityTypeConfiguration<ServiceData>
    {
        public void Configure(EntityTypeBuilder<ServiceData> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("ServiceData");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Description).HasMaxLength(255);
            builder.Property(p => p.ApplicationId).IsRequired();

            builder.HasOne<ApplicationData>().WithMany().HasForeignKey(p => p.ApplicationId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}