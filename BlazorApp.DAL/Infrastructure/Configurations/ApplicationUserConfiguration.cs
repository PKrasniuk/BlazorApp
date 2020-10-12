using BlazorApp.Common.Constants;
using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BlazorApp.DAL.Infrastructure.Configurations
{
    internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser<Guid>>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser<Guid>> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(FieldConstants.QuarterFieldLength);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(FieldConstants.QuarterFieldLength);
            builder.Property(x => x.FullName).IsRequired().HasMaxLength(FieldConstants.BaseFieldLength);
        }
    }
}