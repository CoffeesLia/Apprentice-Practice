using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class KnowledgeConfig : IEntityTypeConfiguration<Knowledge>
    {
        public void Configure(EntityTypeBuilder<Knowledge> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Knowledge");

            builder.HasKey(k => k.Id);

            // Exemplo de índice único considerando apenas MemberId, SquadId, Status
            builder.HasIndex(k => new { k.MemberId, k.SquadId, k.Status }).IsUnique();

            // Definindo valor padrão para ApplicationIds como array vazio
            builder.Property(k => k.ApplicationIds)
                .HasDefaultValueSql("'[]'");

            // relacionamento com member
            builder.HasOne(k => k.Member)
                .WithMany(m => m.Knowledges)
                .HasForeignKey(k => k.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // relacionamento com squad
            builder.HasOne(k => k.Squad)
                .WithMany()
                .HasForeignKey(k => k.SquadId)
                .OnDelete(DeleteBehavior.Cascade);

            // relacionamento muitos-para-muitos com ApplicationData
            builder
                .HasMany(k => k.Applications)
                .WithMany(a => a.Knowledges)
                .UsingEntity(j => j.ToTable("KnowledgeApplications"));
        }
    }
}
