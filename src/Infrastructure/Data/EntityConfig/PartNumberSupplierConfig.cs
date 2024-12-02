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
    public class PartNumberSupplierConfig : IEntityTypeConfiguration<PartNumberSupplier>
    {
        public void Configure(EntityTypeBuilder<PartNumberSupplier> builder)
        {
            builder.ToTable("PartNumberSupplier");

            builder.HasKey(p => new { p.PartNumberId, p.SupplierId });

            builder.Property(p => p.UnitPrice).IsRequired();
            builder.Ignore(p => p.Id);

            builder.HasOne(p => p.PartNumber)
                .WithMany(p => p.PartNumberSupplier)
                .HasForeignKey(p => p.PartNumberId);

            builder.HasOne(p => p.Supplier)
                .WithMany(p => p.PartNumberSupplier)
                .HasForeignKey(p => p.SupplierId);

        }
    }
}

