using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatService.Entities;

namespace ChatService.Configuration;
public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_message");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.Room).HasColumnName("room").HasMaxLength(50).HasDefaultValue("global");
        builder.Property(x => x.SenderId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.SenderName).HasColumnName("user_name").HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.SenderName);//.IsUnique();
        builder.Property(x => x.Text).HasColumnName("content").HasMaxLength(250).IsRequired();
        builder.Property(x => x.SentAt).HasColumnName("sent_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
