using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TaskService.Configurations;
using TaskService.Entities;
using Common.Messaging.Outbox;

namespace TaskService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options) { }

    public DbSet<Tasks> Tasks { get; set; } = null!;
    public DbSet<TaskUser> TaskUser { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new TaskUserConfiguration());
        modelBuilder.AddOutbox("task_outbox");
    }
}