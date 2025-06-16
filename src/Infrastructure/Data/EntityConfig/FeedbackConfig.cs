using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class FeedbackConfig : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Feedback");
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

            builder.HasOne(i => i.Application)
                .WithMany()
                .HasForeignKey(i => i.ApplicationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.Members)
                .WithOne()
                .HasForeignKey(m => m.SquadId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}