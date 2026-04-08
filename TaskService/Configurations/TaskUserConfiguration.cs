using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskService.Entities;

namespace TaskService.Configurations;
public class TaskUserConfiguration : IEntityTypeConfiguration<TaskUser>
{
    public void Configure(EntityTypeBuilder<TaskUser> builder)
    {
        builder.ToTable("task_assignments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.TaskId).HasColumnName("task_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();

        builder.HasIndex(x => new { x.TaskId, x.UserId })
            .IsUnique();
    }
}
