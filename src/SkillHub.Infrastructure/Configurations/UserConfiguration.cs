using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Commons;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("first_name");

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("last_name");

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("role");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasDiscriminator(u => u.Role)
            .HasValue<Participant>(UserRole.Participant)
            .HasValue<Employee>(UserRole.Employee);

        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("email");

            emailBuilder.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Identification, idBuilder =>
        {
            idBuilder.Property(i => i.Type)
                .IsRequired()
                .HasConversion<int>()
                .HasColumnName("identification_type");

            idBuilder.Property(i => i.Number)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("identification_number");

            idBuilder.HasIndex(i => new { i.Type, i.Number }).IsUnique();
        });

        builder.OwnsOne(u => u.PhoneNumber, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.CountryCode)
                .HasMaxLength(10)
                .HasColumnName("phone_country_code");

            phoneBuilder.Property(p => p.Number)
                .HasMaxLength(30)
                .HasColumnName("phone_number");
        });

        builder.OwnsOne(u => u.Address, addrBuilder =>
        {
            addrBuilder.Property(a => a.Street)
                .HasMaxLength(200)
                .HasColumnName("address_street");

            addrBuilder.Property(a => a.City)
                .HasMaxLength(100)
                .HasColumnName("address_city");

            addrBuilder.Property(a => a.State)
                .HasMaxLength(100)
                .HasColumnName("address_state");

            addrBuilder.Property(a => a.PostalCode)
                .HasMaxLength(20)
                .HasColumnName("address_postal_code");

            addrBuilder.Property(a => a.Country)
                .HasMaxLength(100)
                .HasColumnName("address_country");
        });

        builder.Navigation(u => u.Email).IsRequired();
        builder.Navigation(u => u.Identification).IsRequired();
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(e => e.Specialty)
            .HasMaxLength(150)
            .HasColumnName("specialty");
    }
}
