using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Entities;

namespace UserService.Configurations;
public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_session");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("userid").IsRequired();
        builder.Property(x => x.SigninTime).HasColumnName("signintime").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(x => x.SignoutTime).HasColumnName("signouttime");
        builder.Property(x => x.IpAddress).HasColumnName("ipaddress").HasMaxLength(50);
        builder.Property(x => x.UserAgent).HasColumnName("useragent").HasMaxLength(100);
        builder.Property(x => x.IsActive).HasColumnName("isactive").IsRequired();

        builder
            .HasOne(us => us.User)
            .WithMany(us => us.UserSession)
            .HasForeignKey(us => us.UserId)
            .IsRequired();
    }

}
