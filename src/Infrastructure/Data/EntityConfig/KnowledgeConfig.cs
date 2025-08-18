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

            // assegura que não exista duplicidade de associação
            builder.HasIndex(k => new { k.MemberId, k.ApplicationId}).IsUnique();

            // relacionamento com member
            builder.HasOne(k => k.Member)
                .WithMany(m => m.Knowledges)
                .HasForeignKey(k => k.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // relacionamento com application
            builder.HasOne(k => k.Application)
                .WithMany(a => a.Knowledges)
                .HasForeignKey(k => k.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // relacionamento com squad
            builder.HasOne(k => k.Squad)
                .WithMany()
                .HasForeignKey(k => k.SquadId)
                .OnDelete(DeleteBehavior.Cascade);

            // N:N para AssociatedApplications
            builder.HasMany(k => k.AssociatedApplications)
                   .WithMany() // sem criar FK em ApplicationData
                   .UsingEntity<Dictionary<string, object>>(
                       "KnowledgeApplication",
                       j => j.HasOne<ApplicationData>()
                             .WithMany()
                             .HasForeignKey("ApplicationId")
                             .OnDelete(DeleteBehavior.Cascade),
                       j => j.HasOne<Knowledge>()
                             .WithMany()
                             .HasForeignKey("KnowledgeId")
                             .OnDelete(DeleteBehavior.Cascade),
                       j => j.HasKey("KnowledgeId", "ApplicationId")
                   );

            // N:N para AssociatedSquads
            builder.HasMany(k => k.AssociatedSquads)
                   .WithMany()
                   .UsingEntity<Dictionary<string, object>>(
                       "KnowledgeSquad",
                       j => j.HasOne<Squad>()
                             .WithMany()
                             .HasForeignKey("SquadId")
                             .OnDelete(DeleteBehavior.Cascade),
                       j => j.HasOne<Knowledge>()
                             .WithMany()
                             .HasForeignKey("KnowledgeId")
                             .OnDelete(DeleteBehavior.Cascade),
                       j => j.HasKey("KnowledgeId", "SquadId")
                   );
        }
    }
}
