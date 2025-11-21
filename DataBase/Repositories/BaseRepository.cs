using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public abstract class BaseRepository<T>(ApplicationContext context) : IBaseRepository<T> where T : BaseEntity
{
    public virtual async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);

    public virtual async Task<ICollection<T>> GetAllAsync() => await context.Set<T>().ToListAsync();
    
    public virtual async Task<bool> AddAsync(T entity)
    {
        await context.Set<T>().AddAsync(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        context.Set<T>().Update(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        context.Set<T>().Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }
}