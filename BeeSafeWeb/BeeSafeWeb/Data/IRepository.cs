namespace BeeSafeWeb.Data;

public interface IRepository<T>
{
    public List<T> GetAll();
    public T? GetById(Guid id);
    public void Add(T entity);
    public void Update(T entity);
    public void Delete(Guid id);
    public IQueryable<T> GetQueryable();
}