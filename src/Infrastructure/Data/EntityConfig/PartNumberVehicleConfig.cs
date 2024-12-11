using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

