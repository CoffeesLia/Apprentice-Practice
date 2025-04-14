using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class DataServiceConfig : IEntityTypeConfiguration<DataService>
    {
        public void Configure(EntityTypeBuilder<DataService> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Service");
            builder.HasKey(p => p.ServiceId);
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}