using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.EntityConfigurations
{
    public class RefreshSessionEntityConfig : IEntityTypeConfiguration<RefreshSession>
    {
        public void Configure(EntityTypeBuilder<RefreshSession> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .IsRequired(true);
            builder.Property(x => x.RefreshToken)
                   .HasColumnName("RefreshToken");



        }
    }
}