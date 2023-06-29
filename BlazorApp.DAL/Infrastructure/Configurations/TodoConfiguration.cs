using System;
using BlazorApp.Common.Constants;
using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.DAL.Infrastructure.Configurations;

internal class TodoConfiguration : IEntityTypeConfiguration<Todo<Guid>>
{
    public void Configure(EntityTypeBuilder<Todo<Guid>> builder)
    {
        builder.ToTable("Todos");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Property(x => x.Title).IsRequired().HasMaxLength(FieldConstants.HalfFieldLength);
        builder.Property(x => x.IsCompleted).IsRequired().HasDefaultValue(false);
    }
}