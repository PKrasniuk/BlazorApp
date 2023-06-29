using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.DAL.Infrastructure.Configurations;

public class DbLogConfiguration : IEntityTypeConfiguration<DbLog>
{
    public void Configure(EntityTypeBuilder<DbLog> builder)
    {
        builder.ToTable("logs").HasNoKey();

        builder.Property(x => x.Message).HasColumnName("message").IsRequired().HasColumnType("text");
        builder.Property(x => x.MessageTemplate).HasColumnName("message_template").IsRequired()
            .HasColumnType("text");
        builder.Property(x => x.Level).HasColumnName("level").IsRequired().HasColumnType("integer");
        builder.Property(x => x.TimeStamp).HasColumnName("timestamp").IsRequired();
        builder.Property(x => x.Exception).HasColumnName("exception").HasColumnType("text");
        builder.Property(x => x.Properties).HasColumnName("log_event").HasColumnType("jsonb");
    }
}