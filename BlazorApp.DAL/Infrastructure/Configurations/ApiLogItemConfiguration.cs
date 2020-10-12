using BlazorApp.Common.Constants;
using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BlazorApp.DAL.Infrastructure.Configurations
{
    internal class ApiLogItemConfiguration : IEntityTypeConfiguration<ApiLogItem<Guid>>
    {
        public void Configure(EntityTypeBuilder<ApiLogItem<Guid>> builder)
        {
            builder.ToTable("ApiLogs");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.RowVersion).IsRowVersion();

            builder.Property(x => x.RequestTime).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.ResponseMillis).IsRequired();
            builder.Property(x => x.StatusCode).IsRequired();
            builder.Property(x => x.Method).IsRequired();
            builder.Property(x => x.Path).IsRequired().HasMaxLength(FieldConstants.BigFieldLength);
            builder.Property(x => x.QueryString).IsRequired(false).HasMaxLength(FieldConstants.BaseFieldLength);
            builder.Property(x => x.RequestBody).IsRequired(false).HasMaxLength(FieldConstants.BaseFieldLength);
            builder.Property(x => x.ResponseBody).IsRequired(false).HasMaxLength(FieldConstants.BaseFieldLength);
            builder.Property(x => x.IpAddress).IsRequired(false).HasMaxLength(FieldConstants.QuarterFieldLength);
            builder.Property(x => x.ApplicationUserId).IsRequired(false);

            builder.HasIndex(x => x.ApplicationUserId).IsUnique(false);
        }
    }
}