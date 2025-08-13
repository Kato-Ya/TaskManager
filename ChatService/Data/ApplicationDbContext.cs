using Microsoft.EntityFrameworkCore;
using ChatService.Entities;

namespace ChatService.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<ChatMessage> ChatMessage { get; set; } = null!;
}