using System.Linq.Expressions;
using Flipard.Application.Repositories;
using Flipard.Domain.Common;
using Flipard.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Flipard.Persistence.Repositories
{
    public class Repository<T> : IRepository<T> where T : EntityBase<Guid>
    {
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            Table = _context.Set<T>(); 
        }

        // Read işlemleri

        public IQueryable<T> GetAll(bool tracking = true) =>
            tracking ? Table : Table.AsNoTracking();

        public IQueryable<T?> GetWhere(Expression<Func<T, bool>> method, bool tracking = true) =>
            tracking ? Table.Where(method) : Table.Where(method).AsNoTracking();

        public async Task<T?> GetSingleAsync(Expression<Func<T, bool>> method, bool tracking = true) =>
            await GetWhere(method, tracking).FirstOrDefaultAsync();

        public async Task<T?> GetByIdAsync(string id, bool tracking = true) =>
            await GetSingleAsync(x => x.Id == Guid.Parse(id), tracking);

        // Write işlemleri

        public async Task<bool> AddAsync(T entity)
        {
            await Table.AddAsync(entity);
            return true;
        }

        public async Task<bool> AddRangeAsync(List<T> entities)
        {
            await Table.AddRangeAsync(entities);
            return true;
        }

        public bool Update(T entity)
        {
            Table.Update(entity);
            return true;
        }

        public bool Delete(T entity)
        {
            Table.Remove(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            Table.Remove(entity);
            return true;
        }

        public bool DeleteAll(List<T> entities)
        {
            Table.RemoveRange(entities);
            return true;
        }

        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

        public DbSet<T> Table { get; }
    }
}