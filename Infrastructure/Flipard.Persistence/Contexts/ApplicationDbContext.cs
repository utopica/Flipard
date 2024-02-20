using Flipard.Domain.Entities;
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

           
            base.OnModelCreating(modelBuilder); 
        }
    }
}
