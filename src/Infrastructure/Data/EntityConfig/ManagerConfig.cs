using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class ManagerConfig : IEntityTypeConfiguration<Manager>
    {
        public void Configure(EntityTypeBuilder<Manager> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Manager");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
            builder.Property(p => p.Email).IsRequired().HasMaxLength(70);
        }
    }
}