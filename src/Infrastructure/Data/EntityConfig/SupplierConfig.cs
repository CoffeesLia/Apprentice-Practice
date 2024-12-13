using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfig
{
    public class SupplierConfig : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Supplier");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Code).IsRequired().HasMaxLength(11);
            builder.Property(p => p.CompanyName).IsRequired().HasMaxLength(255);
            builder.Property(p => p.Fone).IsRequired().HasMaxLength(20);
            builder.Property(p => p.Address).IsRequired().HasMaxLength(255);

        }
    }
}
