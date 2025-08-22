using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    internal class RepoConfig : IEntityTypeConfiguration<Repo>
    {
        public void Configure(EntityTypeBuilder<Repo> builder)
        {
            builder.ToTable("Repo");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(ad => ad.ApplicationData)
               .WithMany()
               .HasForeignKey(ad => ad.ApplicationId) 
               .OnDelete(DeleteBehavior.Restrict);

            builder.Property(d => d.Url)
                .IsRequired();

            builder.Property(d => d.ApplicationId)
                .IsRequired();

            builder.Property(d => d.Description)
                .IsRequired()
                .HasMaxLength(5000);

            builder.HasOne(d => d.ApplicationData)
                .WithMany()
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
