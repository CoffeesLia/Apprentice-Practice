using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class SquadConfig : IEntityTypeConfiguration<Squad>
    {
        public void Configure(EntityTypeBuilder<Squad> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);

            builder.ToTable("Squads");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(55);

            builder.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(200);

            // Configuração do relacionamento muitos-para-muitos com ApplicationData
            builder
                .HasMany(s => s.Applications)
                .WithMany(a => a.Squads)
                .UsingEntity<Dictionary<string, object>>(
                    "SquadApplication",
                    j => j.HasOne<ApplicationData>().WithMany().HasForeignKey("ApplicationDataId"),
                    j => j.HasOne<Squad>().WithMany().HasForeignKey("SquadId")
                );
        }
    }
}
