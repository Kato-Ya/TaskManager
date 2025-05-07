using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Threading.Tasks;
using UserService.Entities;


namespace UserService.Configurations;
//public class RoleConfiguration : IEntityTypeConfiguration<Roles>
//{
//    public void Configure(EntityTypeBuilder<Roles> builder)
//    {
//        builder.ToTable("roles");
//        builder.HasKey(x => x.Id);

//        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
//        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
//        builder.HasIndex(x => x.Name)
//            .IsUnique();
//        builder.Property(x => x.Description).HasColumnName("description");
//    }

//}

public class RoleConfiguration : IEntityTypeConfiguration<Roles>
{
    public void Configure(EntityTypeBuilder<Roles> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Description).HasColumnName("description");

        builder
            .HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId);
    }
}

