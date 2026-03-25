using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskService.Entities;

namespace TaskService.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<Tasks>
{
    public void Configure(EntityTypeBuilder<Tasks> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(x => x.Priority).HasColumnName("priority").HasMaxLength(20).HasDefaultValue("Medium");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id").IsRequired();
        //builder.Property(x => x.AssigneeId).HasColumnName("assignee_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.DueDate).HasColumnName("due_date");

        //builder.HasOne<Users>()
        //    .WithMany()
        //    .HasForeignKey(x => x.CreatorId)
        //    .OnDelete(DeleteBehavior.Cascade);

        //builder.HasOne<Users>()
        //    .WithMany()
        //    .HasForeignKey(x => x.AssigneeId)
        //    .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.TaskUsers)
            .WithOne(x => x.Task)
            .HasForeignKey(x => x.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}