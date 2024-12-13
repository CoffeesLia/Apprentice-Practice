using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfig
{
    public class PartNumberVehicleConfig : IEntityTypeConfiguration<PartNumberVehicle>
    {
        public void Configure(EntityTypeBuilder<PartNumberVehicle> builder)
        {
            builder.ToTable("PartNumberVehicle");

            builder.HasKey(p => new { p.PartNumberId, p.VehicleId });

            builder.Property(p => p.Amount).IsRequired();
            builder.Ignore(p => p.Id);





        }
    }

}

