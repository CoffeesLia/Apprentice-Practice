using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    internal class DocumentDataConfig : IEntityTypeConfiguration<DocumentData>
    {
        public void Configure(EntityTypeBuilder<DocumentData> builder)
        {
            builder.ToTable("DocumentData");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(ad => ad.ApplicationData)
               .WithMany(a => a.Documents)
               .HasForeignKey(ad => ad.ApplicationId);

            builder.Property(d => d.Url)
                .IsRequired();

            builder.Property(d => d.ApplicationId)
                .IsRequired();

            builder.HasOne(d => d.ApplicationData)
                .WithMany()
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
