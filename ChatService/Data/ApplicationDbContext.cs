using Microsoft.EntityFrameworkCore;
using ChatService.Entities;
using ChatService.Configuration;

namespace ChatService.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<ChatMessage> ChatMessage { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
    }

}