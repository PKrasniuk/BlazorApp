using System;
using BlazorApp.Common.Constants;
using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.DAL.Infrastructure.Configurations;

internal class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile<Guid>>
{
    public void Configure(EntityTypeBuilder<UserProfile<Guid>> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.LastPageVisited).IsRequired().HasMaxLength(FieldConstants.BigFieldLength)
            .HasDefaultValue("/");
        builder.Property(x => x.IsNavOpen).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsNavMinified).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.Count).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.LastUpdatedDate).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}