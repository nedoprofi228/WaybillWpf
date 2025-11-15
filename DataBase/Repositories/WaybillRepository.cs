using Microsoft.EntityFrameworkCore;
using WaybillWpf.Core.Entities;
using WaybillWpf.Core.Interfaces;

namespace WaybillWpf.DataBase;

public class WaybillRepository(ApplicationContext context) : BaseRepository<Waybill>(context), IWaybillsRepository
{
    public override async Task<ICollection<Waybill>> GetAllAsync() =>
        await context.Waybills       
            .Include(w => w.WaybillDetails)
            .Include(w => w.User)
            .Include(w => w.Driver)
            .ToListAsync();

    public override async Task<Waybill?> GetByIdAsync(int id) => await context.Waybills
        .Include(w => w.WaybillDetails)
        .Include(w => w.User)
        .Include(w => w.Driver)
        .FirstOrDefaultAsync(w => w.UserId == id);

    public async Task<ICollection<Waybill>> GetWaybillsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.User)
            .Include(w => w.Driver)
            .Where(w => w.WaybillDetails.Any(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate))
            .ToListAsync();
    }

    public async Task<ICollection<Waybill>> GetWaybillsByUserIdAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.User)
            .Include(w => w.Driver)
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
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }
}