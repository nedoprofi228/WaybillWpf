using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public abstract class BaseRepository<T>(ApplicationContext context) : IBaseRepository<T> where T : BaseEntity
{
    public virtual async Task<T?> GetByIdAsync(int id) => await context.Set<T>().AsNoTracking().FirstOrDefaultAsync((e) => e.Id == id);

    public virtual async Task<ICollection<T>> GetAllAsync() => await context.Set<T>().AsNoTracking().ToListAsync();

    public virtual async Task<bool> AddAsync(T entity)
    {
        await context.Set<T>().AddAsync(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        var local = context.Set<T>().Local.FirstOrDefault(entry => entry.Id.Equals(entity.Id));
        if (local != null)
        {
            context.Entry(local).State = EntityState.Detached;
        }

        context.Set<T>().Update(entity);
        return await context.SaveChangesAsync() > 0;
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        context.Set<T>().Remove(entity);
        return await context.SaveChangesAsync() > 0;
    }
}