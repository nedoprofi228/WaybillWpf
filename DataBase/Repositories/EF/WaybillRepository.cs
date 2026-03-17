using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class WaybillRepository(ApplicationContext context) : BaseRepository<Waybill>(context), IWaybillsRepository
{
    public override async Task<ICollection<Waybill>> GetAllAsync() => LinkDetails(
        await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.Logist)
            .Include(w => w.Driver)
            .Include(w => w.Car)
                .ThenInclude(c => c.FuelType)
            .Include(w => w.WaybillTasks)
            .ToListAsync());

    public override async Task<Waybill?> GetByIdAsync(int id) => LinkDetails(await context.Waybills
        .Include(w => w.WaybillDetails)
        .Include(w => w.Logist)
        .Include(w => w.Driver)
        .Include(w => w.Car)
            .ThenInclude(c => c.FuelType)
        .Include(w => w.WaybillTasks)
        .FirstOrDefaultAsync(w => w.Id == id));

    public async Task<ICollection<Waybill>> GetWaybillsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return LinkDetails(await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.Logist)
            .Include(w => w.Driver)
            .Include(w => w.Car)
                .ThenInclude(c => c.FuelType)
            .Include(w => w.WaybillTasks)
            .Where(w => w.WaybillDetails.Any(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate))
            .ToListAsync());
    }

    public async Task<ICollection<Waybill>> GetWaybillsByUserIdAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return LinkDetails(await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.Logist)
            .Include(w => w.Driver)
            .Include(w => w.Car)
                .ThenInclude(c => c.FuelType)
            .Include(w => w.WaybillTasks)
            .Where(w => w.LogistId == userId &&
                        w.WaybillDetails.Any(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate))
            .ToListAsync());
    }

    public async Task<ICollection<Waybill>> GetWaybillsByUserIdAsync(int userId)
    {
        return LinkDetails(await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.Logist)
            .Include(w => w.Driver)
            .Include(w => w.Car)
                .ThenInclude(c => c.FuelType)
            .Include(w => w.WaybillTasks)
            .Where(w => w.LogistId == userId)
            .ToListAsync());
    }

    public async Task<ICollection<Waybill>> GetWaybillsByDriverIdAsync(int driverId)
    {
        return LinkDetails(await context.Waybills
            .Include(w => w.WaybillDetails)
            .Include(w => w.Logist)
            .Include(w => w.Driver)
            .Include(w => w.Car)
                .ThenInclude(c => c.FuelType)
            .Include(w => w.WaybillTasks)
            .Where(w => w.DriverId == driverId)
            .ToListAsync());
    }

    private ICollection<Waybill> LinkDetails(ICollection<Waybill> waybills)
    {
        foreach (var w in waybills)
        {
            if (w.WaybillDetails != null)
            {
                foreach (var d in w.WaybillDetails)
                {
                    d.Waybill = w;
                }
            }
        }
        return waybills;
    }

    private Waybill? LinkDetails(Waybill? w)
    {
        if (w?.WaybillDetails != null)
        {
            foreach (var d in w.WaybillDetails)
            {
                d.Waybill = w;
            }
        }
        return w;
    }
}