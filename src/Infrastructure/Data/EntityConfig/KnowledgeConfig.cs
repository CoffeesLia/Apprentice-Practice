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
                .WithMany()
                .HasForeignKey(k => k.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // relacionamento com application
            builder.HasOne(k => k.Application)
                .WithMany()
                .HasForeignKey(k => k.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
