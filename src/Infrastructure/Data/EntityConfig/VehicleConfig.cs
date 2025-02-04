using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class VehicleConfig : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Vehicle");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Chassi).IsRequired().HasMaxLength(17);

        }
    }
}
