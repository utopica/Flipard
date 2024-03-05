using Flipard.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flipard.Application.Repositories
{
    public interface IWriteRepository<T> : IRepository<T> where T : EntityBase<Guid>
    {
        Task<bool> AddAsync(T entity);
        Task<bool> AddRangeAsync(List<T> entities);
        bool Update(T entity);
        bool Delete(T entity);
        Task<bool> DeleteAsync(string id);
        bool DeleteAll(List<T> entities);

        Task<int> SaveAsync();
    }
}