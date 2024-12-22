using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Flipard.Domain.Common;

namespace Flipard.Application.Repositories
{
    public interface IRepository<T> where T : EntityBase<Guid>
    {
        DbSet<T> Table { get; }
        
        // Read operations
        IQueryable<T> GetAll(bool tracking = true);
        IQueryable<T?> GetWhere(Expression<Func<T, bool>> method, bool tracking = true);
        Task<T?> GetSingleAsync(Expression<Func<T, bool>> method, bool tracking = true);
        Task<T?> GetByIdAsync(string id, bool tracking = true);

        // Write operations
        Task<bool> AddAsync(T entity);
        Task<bool> AddRangeAsync(List<T> entities);
        bool Update(T entity);
        bool Delete(T entity);
        Task<bool> DeleteAsync(string id);
        bool DeleteAll(List<T> entities);

        // Save changes
        Task<int> SaveAsync();
    }
}
