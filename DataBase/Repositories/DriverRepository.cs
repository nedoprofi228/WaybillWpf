using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class DriverRepository(ApplicationContext context) : BaseRepository<Driver>(context), IDriversRepository
{
    public override async Task<ICollection<Driver>> GetAllAsync()
    {
        return await context.Drivers
            .Include(d => d.DriveLicense)
            .ToListAsync();
    }

    public override async Task<Driver?> GetByIdAsync(int id)
    {
        return await context.Drivers
            .Include(d => d.DriveLicense)
            .FirstAsync();
    }
}