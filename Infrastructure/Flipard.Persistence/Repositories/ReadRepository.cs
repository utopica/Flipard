using Flipard.Application.Repositories;
using Flipard.Domain.Common;
using Flipard.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Persistence.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : EntityBase<Guid>
    {
        private readonly ApplicationDbContext _context;

        public ReadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public DbSet<T> Table => _context.Set<T>();

        public IQueryable<T> GetAll(bool tracking = true)
        {
            var query = Table.AsQueryable(); //iqueryableda çalıştığımız için table'ı query'e atıyoruz

            if (!tracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }

        public async Task<T> GetByIdAsync(string id, bool tracking = true)
        {
            //=> await Table.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id));

            var query = Table.AsQueryable();

            if (!tracking)
            {
                query = Table.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id));


        }

        public Task<bool> AddAsync(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddRangeAsync(List<T> entities)
        {
            throw new NotImplementedException();
        }

        public bool Update(T entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAll(List<T> entities)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetSingleAsync(Expression<Func<T, bool>> method, bool tracking = true)
        {
            var query = Table.AsQueryable();

            if (!tracking)
            {
                query = Table.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(method);
        }


        public IQueryable<T> GetWhere(Expression<Func<T, bool>> method, bool tracking = true)
        {
            var query = Table.Where(method);

            if (!tracking)
            {
                query = query.AsNoTracking();

            }

            return query;
        }



    }
}
