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
            builder.ToTable("Incident"); // Nome da tabela no banco de dados
            builder.HasKey(i => i.Id); // Chave primária

            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(200); // Tamanho máximo do título

            builder.Property(i => i.Description)
                .IsRequired(); // Descrição é obrigatória

            builder.Property(i => i.ApplicationId)
                .IsRequired(); // ApplicationId é obrigatório

            builder.Property(i => i.CreatedAt)
                .IsRequired(); // Data de criação é obrigatória

            builder.Property(i => i.Status)
                .IsRequired(); // Status é obrigatório

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