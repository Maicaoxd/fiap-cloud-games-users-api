using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UsersAPI.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", table =>
                table.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('User', 'Administrator')"));

            builder.HasKey(user => user.Id);

            builder.Property(user => user.Id)
                .HasColumnName("Id")
                .ValueGeneratedNever();

            builder.Property(user => user.Name)
                .HasColumnName("Name")
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(user => user.Email)
                .HasColumnName("Email")
                .HasMaxLength(254)
                .HasConversion(
                    email => email.Value,
                    value => Email.Create(value))
                .IsRequired();

            builder.HasIndex(user => user.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.Property(user => user.Cpf)
                .HasColumnName("Cpf")
                .HasMaxLength(11)
                .HasConversion(
                    cpf => cpf.Value,
                    value => Cpf.Create(value))
                .IsRequired();

            builder.HasIndex(user => user.Cpf)
                .IsUnique()
                .HasDatabaseName("IX_Users_Cpf");

            builder.Property(user => user.BirthDate)
                .HasColumnName("BirthDate")
                .IsRequired();

            builder.Property(user => user.PasswordHash)
                .HasColumnName("PasswordHash")
                .HasMaxLength(500)
                .HasConversion(
                    passwordHash => passwordHash.Value,
                    value => PasswordHash.Create(value))
                .IsRequired();

            builder.Property(user => user.Role)
                .HasColumnName("Role")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(user => user.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            builder.Property(user => user.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired();

            builder.Property(user => user.CreatedBy)
                .HasColumnName("CreatedBy");

            builder.Property(user => user.UpdatedAt)
                .HasColumnName("UpdatedAt");

            builder.Property(user => user.UpdatedBy)
                .HasColumnName("UpdatedBy");
        }
    }
}
