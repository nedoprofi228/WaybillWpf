namespace WaybillWpf.Domain.Interfaces;

public interface IBaseRepository<T>
{
    public Task<T?> GetByIdAsync(int id);
    public Task<ICollection<T>> GetAllAsync();
    public Task<bool> AddAsync(T entity);
    public Task<bool> UpdateAsync(T entity);
    public Task<bool> DeleteAsync(T entity);
}