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

            builder.HasIndex(k => new { k.MemberId, k.SquadId, k.Status }).IsUnique();

            builder.Property(k => k.ApplicationIds)
                .HasDefaultValueSql("'[]'");

            builder.HasOne(k => k.Member)
                .WithMany(m => m.Knowledges)
                .HasForeignKey(k => k.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(k => k.Squad)
                .WithMany()
                .HasForeignKey(k => k.SquadId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasMany(k => k.Applications)
                .WithMany(a => a.Knowledges)
                .UsingEntity(j => j.ToTable("KnowledgeApplications"));
        }
    }
}
