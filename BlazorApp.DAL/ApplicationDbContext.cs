using BlazorApp.Common.Interfaces;
using BlazorApp.DAL.Infrastructure.Configurations;
using BlazorApp.DAL.Infrastructure.Extensions;
using BlazorApp.DAL.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorApp.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser<Guid>, IdentityRole<Guid>, Guid>,
        IApplicationDbContext
    {
        private static readonly MethodInfo SetGlobalQueryForSoftDeleteMethodInfo = typeof(ApplicationDbContext)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForSoftDelete");

        private static readonly MethodInfo SetGlobalQueryForEntityMethodInfo = typeof(ApplicationDbContext)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == "SetGlobalQueryForEntity");

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserSession<Guid> userSession) :
            base(options)
        {
            UserSession = userSession;
        }

        private IUserSession<Guid> UserSession { get; }
        public DbSet<ApiLogItem<Guid>> ApiLogs { get; set; }

        public DbSet<UserProfile<Guid>> UserProfiles { get; set; }

        public DbSet<Todo<Guid>> Todos { get; set; }

        public DbSet<Message<Guid>> Messages { get; set; }

        public DbSet<DbLog> Logs { get; set; }

        public override int SaveChanges()
        {
            ChangeTracker.SetShadowProperties(UserSession);
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.SetShadowProperties(UserSession);
            return await base.SaveChangesAsync(true, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message<Guid>>()
                .HasOne(a => a.Sender)
                .WithMany(m => m.Messages)
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<ApplicationUser<Guid>>()
                .HasOne(a => a.Profile)
                .WithOne(b => b.ApplicationUser)
                .HasForeignKey<UserProfile<Guid>>(b => b.UserId);

            modelBuilder.ShadowProperties();

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ApiLogItemConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
            modelBuilder.ApplyConfiguration(new MessageConfiguration());
            modelBuilder.ApplyConfiguration(new TodoConfiguration());
            modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
            modelBuilder.ApplyConfiguration(new DbLogConfiguration());

            SetGlobalQueryFilters(modelBuilder);
        }

        private void SetGlobalQueryFilters(ModelBuilder modelBuilder)
        {
            foreach (var tp in modelBuilder.Model.GetEntityTypes())
            {
                var t = tp.ClrType;

                if (typeof(ISoftDelete).IsAssignableFrom(t))
                    SetGlobalQueryForSoftDeleteMethodInfo.MakeGenericMethod(t)
                        .Invoke(this, new object[] { modelBuilder });

                if (typeof(Entity<Guid>).IsAssignableFrom(t))
                    SetGlobalQueryForEntityMethodInfo.MakeGenericMethod(t).Invoke(this, new object[] { modelBuilder });
            }
        }

        public void SetGlobalQueryForSoftDelete<T>(ModelBuilder builder) where T : class, ISoftDelete
        {
            builder.Entity<T>().HasQueryFilter(item => !EF.Property<bool>(item, "IsDeleted"));
        }

        public void SetGlobalQueryForEntity<T>(ModelBuilder builder) where T : Entity<Guid>
        {
            builder.Entity<T>().Property(_ => _.RowVersion).HasColumnName("xmin").HasColumnType("xid")
                .HasConversion(new ValueConverter<byte[], long>(v => BitConverter.ToInt64(v, 0),
                    v => BitConverter.GetBytes(v)))
                .Metadata.SetValueComparer(new ValueComparer<byte[]>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToHashSet().ToArray()));
        }
    }
}