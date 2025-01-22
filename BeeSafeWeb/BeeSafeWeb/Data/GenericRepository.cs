using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeSafeWeb.Data;

public class GenericRepository<T> : IRepository<T> where T : class
{
    private readonly DbSet<T> _table;
    private readonly BeeSafeContext _context;
    
    public GenericRepository(BeeSafeContext context)
    {
        _table = context.Set<T>();
        _context = context;
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _table.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _table.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _table.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _table.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _table.FindAsync(id);
        if (existing != null)
        {
            _table.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }

    public IQueryable<T> GetQueryable()
    {
        return _table.AsQueryable();
    }
}