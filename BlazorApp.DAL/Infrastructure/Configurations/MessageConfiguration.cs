using System;
using BlazorApp.Common.Constants;
using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.DAL.Infrastructure.Configurations;

internal class MessageConfiguration : IEntityTypeConfiguration<Message<Guid>>
{
    public void Configure(EntityTypeBuilder<Message<Guid>> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Property(x => x.UserName).IsRequired().HasMaxLength(FieldConstants.BaseFieldLength);
        builder.Property(x => x.Text).IsRequired().HasMaxLength(FieldConstants.BigFieldLength);
        builder.Property(x => x.When).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
    }
}