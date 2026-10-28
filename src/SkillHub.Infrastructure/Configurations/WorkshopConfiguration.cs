using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Configurations;

public class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.ToTable("workshops");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("code");

        builder.HasIndex(w => w.Code).IsUnique();

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("title");

        builder.Property(w => w.Description)
            .IsRequired()
            .HasMaxLength(2000)
            .HasColumnName("description");

        builder.Property(w => w.StartAt)
            .IsRequired()
            .HasColumnName("start_at");

        builder.Property(w => w.EndAt)
            .IsRequired()
            .HasColumnName("end_at");

        builder.Property(w => w.MaxCapacity)
            .IsRequired()
            .HasColumnName("max_capacity");

        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(w => w.Instructor)
            .WithMany(e => e.InstructedWorkshops)
            .HasForeignKey(w => w.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Enrollments)
            .WithOne(e => e.Workshop)
            .HasForeignKey(e => e.WorkshopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
