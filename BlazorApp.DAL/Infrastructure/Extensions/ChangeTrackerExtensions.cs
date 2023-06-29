using System;
using BlazorApp.Common.Interfaces;
using BlazorApp.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BlazorApp.DAL.Infrastructure.Extensions;

public static class ChangeTrackerExtensions
{
    public static void SetShadowProperties(this ChangeTracker changeTracker, IUserSession<Guid> userSession)
    {
        changeTracker.DetectChanges();
        var userId = Guid.Empty;
        var timestamp = DateTime.UtcNow;

        if (userSession.UserId != Guid.Empty) userId = userSession.UserId;

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.Entity is IAuditable)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedOn").CurrentValue = timestamp;
                    entry.Property("CreatedBy").CurrentValue = userId;
                }

                if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                {
                    entry.Property("ModifiedOn").CurrentValue = timestamp;
                    entry.Property("ModifiedBy").CurrentValue = userId;
                }
            }

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete)
            {
                entry.State = EntityState.Modified;
                entry.Property("IsDeleted").CurrentValue = true;
            }
        }
    }
}