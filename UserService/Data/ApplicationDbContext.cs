using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UserService.Configurations;
using UserService.Entities;

namespace UserService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Roles> Roles { get; set; } = null!;
    public DbSet<Users> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());

        //modelBuilder.Entity<Roles>().ToTable("roles");
        //modelBuilder.Entity<Users>().ToTable("users");

    }
}
