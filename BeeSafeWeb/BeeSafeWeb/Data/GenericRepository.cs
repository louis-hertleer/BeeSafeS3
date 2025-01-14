using Microsoft.EntityFrameworkCore;

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
    public List<T> GetAll()
    {
        return _table.ToList();
    }

    public T? GetById(Guid id)
    {
        return _table.Find(id);
    }

    public void Add(T entity)
    {
        _table.Add(entity);
        _context.SaveChanges();
    }

    public void Update(T entity)
    {
        _table.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        _context.SaveChanges();
    }

    public void Delete(Guid id)
    {
        T? existing = _table.Find(id);
        if (existing != null)
            _table.Remove(existing);
    }

    public IQueryable<T> GetQueryable()
    {
        return _table.AsQueryable();
    }
}