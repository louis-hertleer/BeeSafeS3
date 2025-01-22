using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeSafeWeb.Data;

public interface IRepository<T>
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    IQueryable<T> GetQueryable();
}