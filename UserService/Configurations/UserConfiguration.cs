using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Entities;

namespace UserService.Configurations;

//public class UserConfiguration : IEntityTypeConfiguration<Users>
//{
//    public void Configure(EntityTypeBuilder<Users> builder)
//    {
//        builder.ToTable("users");
//        builder.HasKey(x => x.Id);
//        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
//        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
//        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
//        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
//        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

//        builder
//            .HasMany(u => u.Roles)
//            .WithMany(r => r.Users)
//            .UsingEntity<Dictionary<string, object>>(
//                "user_roles",
//                u => u.HasOne<Roles>().WithMany().HasForeignKey("role_id"),
//                r => r.HasOne<Users>().WithMany().HasForeignKey("user_id"),
//                ur =>
//                {
//                    ur.HasKey("user_id", "role_id");
//                    ur.ToTable("user_roles");
//                });

//    }
//}

public class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.State).HasColumnName("state").IsRequired();

        builder
            .HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder
            .HasMany(u => u.UserSession)
            .WithOne(u => u.User)
            .HasForeignKey(u => u.UserId);
    }
}
