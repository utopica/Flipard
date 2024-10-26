using Flipard.Domain.Entities;
using Flipard.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Persistence.Contexts.Identity
{
    public class IdentityContext : IdentityDbContext<User,Role,Guid>
    {
        //NO NEED TO CODE DBSETS 

        public IdentityContext(DbContextOptions<IdentityContext> dbContextOptions) : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Ignore<Deck>();
            modelBuilder.Ignore<Card>();
            modelBuilder.Ignore<Vocabulary>();
            modelBuilder.Ignore<QuizAttempt>();
            modelBuilder.Ignore<QuizAnswer>();
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
