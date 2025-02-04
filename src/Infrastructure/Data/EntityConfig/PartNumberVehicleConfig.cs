using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class PartNumberVehicleConfig : IEntityTypeConfiguration<VehiclePartNumber>
    {
        public void Configure(EntityTypeBuilder<VehiclePartNumber> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("PartNumberVehicle");

            builder.HasKey(p => new { p.PartNumberId, p.VehicleId });

            builder.Property(p => p.Amount).IsRequired();
        }
    }
}
