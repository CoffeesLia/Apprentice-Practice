using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class PartNumberSupplierConfig : IEntityTypeConfiguration<PartNumberSupplier>
    {
        public void Configure(EntityTypeBuilder<PartNumberSupplier> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("PartNumberSupplier");

            builder.HasKey(p => new { p.PartNumberId, p.SupplierId });

            builder.Property(p => p.UnitPrice).IsRequired();

            builder.HasOne(p => p.PartNumber)
                .WithMany(p => p.Suppliers)
                .HasForeignKey(p => p.PartNumberId);

            builder.HasOne(p => p.Supplier)
                .WithMany(p => p.PartNumbers)
                .HasForeignKey(p => p.SupplierId);
        }
    }
}