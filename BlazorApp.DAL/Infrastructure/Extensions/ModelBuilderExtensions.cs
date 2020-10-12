using BlazorApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace BlazorApp.DAL.Infrastructure.Extensions
{
    public static class ModelBuilderExtensions
    {
        private static readonly MethodInfo SetIsDeletedShadowPropertyMethodInfo = typeof(ModelBuilderExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(t => t.IsGenericMethod && t.Name == "SetIsDeletedShadowProperty");

        private static readonly MethodInfo SetAuditingShadowPropertiesMethodInfo = typeof(ModelBuilderExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(t => t.IsGenericMethod && t.Name == "SetAuditingShadowProperties");

        public static void ShadowProperties(this ModelBuilder modelBuilder)
        {
            foreach (var tp in modelBuilder.Model.GetEntityTypes())
            {
                var t = tp.ClrType;

                if (typeof(IAuditable).IsAssignableFrom(t))
                    SetAuditingShadowPropertiesMethodInfo.MakeGenericMethod(t)
                        .Invoke(modelBuilder, new object[] { modelBuilder });

                if (typeof(ISoftDelete).IsAssignableFrom(t))
                    SetIsDeletedShadowPropertyMethodInfo.MakeGenericMethod(t)
                        .Invoke(modelBuilder, new object[] { modelBuilder });
            }
        }

        public static void SetIsDeletedShadowProperty<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            builder.Entity<T>().Property<bool>("IsDeleted");
        }

        public static void SetAuditingShadowProperties<T>(ModelBuilder builder) where T : class, IAuditable
        {
            builder.Entity<T>().Property<DateTime>("CreatedOn").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<T>().Property<DateTime>("ModifiedOn").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<T>().Property<Guid>("CreatedBy");
            builder.Entity<T>().Property<Guid>("ModifiedBy");

            // The weakening auditable of entity
            // builder.Entity<T>().HasOne<ApplicationUser<Guid>>().WithMany().HasForeignKey("CreatedBy").OnDelete(DeleteBehavior.Restrict);
            //builder.Entity<T>().HasOne<ApplicationUser<Guid>>().WithMany().HasForeignKey("ModifiedBy").OnDelete(DeleteBehavior.Restrict);
        }
    }
}