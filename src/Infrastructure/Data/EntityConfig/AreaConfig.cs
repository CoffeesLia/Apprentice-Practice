using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{

    public class AreaConfig : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Area");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
            builder.Property(p => p.ManagerId).IsRequired();

            builder.HasOne<ApplicationData>().WithMany().HasForeignKey(p => p.ManagerId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}