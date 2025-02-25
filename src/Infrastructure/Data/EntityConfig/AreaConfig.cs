using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;

namespace Stellantis.ProjectName.Infrastructure.Data.EntityConfig
{

        public class AreaConfig : IEntityTypeConfiguration<Area>
        {
            public void Configure(EntityTypeBuilder<Area> builder)
            {
                ArgumentNullException.ThrowIfNull(builder);
                builder.ToTable("Area");
                builder.HasKey(p => p.Id);
                builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
            }
        }
    }