
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{

    public class GitRepoConfig : IEntityTypeConfiguration<GitRepo>
    {
        public void Configure(EntityTypeBuilder<GitRepo> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("GitLabRepository");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
        }
    }
}

