using Flipard.Domain.Common;
using Flipard.Domain.Entities;
using Flipard.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Card>  Cards { get; set; }
        public DbSet<Vocabulary> Vocabularies { get; set;}
        public DbSet<Deck> Decks { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Ignore<User>();
            modelBuilder.Ignore<Role>();
            modelBuilder.Ignore<UserSetting>();

            base.OnModelCreating(modelBuilder); 
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var datas = ChangeTracker.Entries<EntityBase<Guid>>();

            foreach (var data in datas)
            {
                _ = data.State switch
                {
                    EntityState.Modified => data.Entity.ModifiedOn = DateTimeOffset.UtcNow,

                    EntityState.Added => data.Entity.CreatedOn = DateTimeOffset.UtcNow,

                    EntityState.Deleted => data.Entity.DeletedOn = DateTimeOffset.UtcNow,
                };
            }

            return await base.SaveChangesAsync(cancellationToken);

        }
    }
}
