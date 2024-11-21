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
    public class UserEntityConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Surname)
            .HasColumnName("Surname")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.Name)
            .HasColumnName("Name")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.Patronymic)
            .HasColumnName("Patronymic")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.PhoneNumber)
            .HasColumnName("Phone Number")
            .HasColumnType("VARCHAR")
            .HasMaxLength(12)
            .IsRequired();

            builder.Property(x => x.Password)
            .HasColumnName("Password")
            .HasColumnType("VARCHAR")
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.PassportData)
            .HasColumnName("PassportData")
            .HasColumnType("VARCHAR")
            .HasMaxLength(11)
            .IsRequired();

            builder.Property(x => x.Balance)
            .HasColumnName("Balance")
            .HasColumnType("NUMERIC")
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.Status)
            .HasColumnName("Status")
            .HasColumnType("BOOL")
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.RefreshToken)
                .HasColumnName("RefreshToken");

            builder
                .HasMany(x => x.Roles)
                .WithMany(x => x.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    role => role
                        .HasOne<Role>()
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade),
                    user => user
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade));
        }
    }
}
