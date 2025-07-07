using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class IncidentConfig : IEntityTypeConfiguration<Incident>
    {
        public void Configure(EntityTypeBuilder<Incident> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Incident"); 
            builder.HasKey(i => i.Id); 

            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(200); 

            builder.Property(i => i.Description)
                .IsRequired(); 

            builder.Property(i => i.ApplicationId)
                .IsRequired(); 

            builder.Property(i => i.CreatedAt)
                .IsRequired(); 

            builder.Property(i => i.Status)
                .IsRequired();

            builder.HasOne(i => i.Application) // Configura o relacionamento com ApplicationData
                .WithMany() // Sem propriedade de navegação explícita
                .HasForeignKey(i => i.ApplicationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            builder
                .HasMany(i => i.Members)
                .WithMany() // ou .WithMany(m => m.Incidents) se houver navegação reversa
                .UsingEntity(j => j.ToTable("IncidentMembers"));
        }
    }
}