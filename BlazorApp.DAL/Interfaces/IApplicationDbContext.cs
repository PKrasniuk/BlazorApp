using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorApp.DAL.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<ApiLogItem<Guid>> ApiLogs { get; set; }

        public DbSet<UserProfile<Guid>> UserProfiles { get; set; }

        public DbSet<Todo<Guid>> Todos { get; set; }

        public DbSet<Message<Guid>> Messages { get; set; }

        public DbSet<ApplicationUser<Guid>> Users { get; set; }

        public DbSet<DbLog> Logs { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        public int SaveChanges();
    }
}