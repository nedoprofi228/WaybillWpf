using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class WaybillRepository(ApplicationContext context) : BaseRepository<Waybill>(context), IWaybillsRepository
{
    public override async Task<ICollection<Waybill>> GetAllAsync() =>
        await context.Waybills       
            .Include(w => w.WaybillDetails)
            .Include(w => w.User)
            .Include(w => w.Driver)
            .Include(w => w.Car)
            .Include(w => w.WaybillTasks)
            .ToListAsync();

    public override async Task<Waybill?> GetByIdAsync(int id) => await context.Waybills
        .Include(w => w.WaybillDetails)
        .Include(w => w.User)
        .Include(w => w.Driver)
        .Include(w => w.Car)
        .Include(w => w.WaybillTasks)
        .FirstOrDefaultAsync(w => w.Id == id);

    public async Task<ICollection<Waybill>> GetWaybillsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.User)
            .Include(w => w.Driver)
            .Include(w => w.Car)
            .Include(w => w.WaybillTasks)
            .Where(w => w.WaybillDetails.Any(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate))
            .ToListAsync();
    }

    public async Task<ICollection<Waybill>> GetWaybillsByUserIdAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.User)
            .Include(w => w.Driver)
            .Include(w => w.Car)
            .Include(w => w.WaybillTasks)
            .Where(w => w.UserId == userId &&
                        w.WaybillDetails.Any(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate))
            .ToListAsync();
    }

    public async Task<ICollection<Waybill>> GetWaybillsByUserIdAsync(int userId)
    {
        return await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.User)
            .Include(w => w.Driver)
            .Include(w => w.Car)
            .Include(w => w.WaybillTasks)
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }
}