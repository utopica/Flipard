using Flipard.Domain.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flipard.Persistence.Contexts;
using Flipard.Application.Repositories;

namespace Flipard.Persistence.Repositories
{
    public class WriteRepository<T> : IWriteRepository<T> where T : EntityBase<Guid>
    {
        private readonly ApplicationDbContext _context;

        public WriteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public async Task<bool> AddAsync(T entity)
        {
            EntityEntry<T> entityEntry = await Table.AddAsync(entity);

            return entityEntry.State == EntityState.Added;
        }

        public async Task<bool> AddRangeAsync(List<T> entities)
        {
            try
            {
                await Table.AddRangeAsync(entities);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Delete(T entity)
        {
            EntityEntry<T> entityEntry = Table.Remove(entity);

            return entityEntry.State == EntityState.Deleted;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            T model = await Table.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id));

            return Delete(model);
        }

        public bool DeleteAll(List<T> entities)
        {
            Table.RemoveRange(entities);

            return true;
        }

        public bool Update(T entity)
        {
            EntityEntry entityEntry = Table.Update(entity);
            return entityEntry.State == EntityState.Modified;
        }

        public async Task<int> SaveAsync()
            => await _context.SaveChangesAsync();





    }
}
