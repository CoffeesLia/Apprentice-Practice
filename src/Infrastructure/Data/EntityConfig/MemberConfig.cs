using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{
    public class MemberConfig : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.ToTable("Members");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Role).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Cost).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(x => x.Email).IsRequired().HasMaxLength(150);
        }
    }
}
