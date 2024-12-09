using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;

namespace Project.Infrastructure.EntityConfigurations
{
    public class BotInputEntityConfig : IEntityTypeConfiguration<BotInputData>
    {
        public void Configure(EntityTypeBuilder<BotInputData> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserIP)
                .HasColumnName("UserIP")
                .HasColumnType("VARCHAR")
                .HasMaxLength(50)
                .IsRequired(true);
            builder.Property(x => x.InputPhone)
                .HasColumnName("InputPhone")
                .HasColumnType("VARCHAR")
                .HasMaxLength(50)
                .IsRequired(true);
        }
    }
}
