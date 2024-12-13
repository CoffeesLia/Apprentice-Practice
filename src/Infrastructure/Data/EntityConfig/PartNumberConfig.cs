using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfig
{
    public class PartNumberConfig : IEntityTypeConfiguration<PartNumber>
    {
        public void Configure(EntityTypeBuilder<PartNumber> builder)
        {
            builder.ToTable("PartNumber");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Code).IsRequired().HasMaxLength(11);
            builder.Property(p => p.Description).IsRequired().HasMaxLength(255);
            builder.Property(p => p.Type).IsRequired().HasMaxLength(1);



        }
    }

}
