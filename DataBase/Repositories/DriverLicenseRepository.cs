using Microsoft.EntityFrameworkCore;
using WaybillWpf.Core.Entities;
using WaybillWpf.Core.Interfaces;

namespace WaybillWpf.DataBase;

public class DriverLicenseRepository(ApplicationContext context) : BaseRepository<DriveLicense>(context), IDriverLicensesRepository
{
    public async Task<ICollection<DriveLicense>> GetAllLicensesBydriverIdAsync(int driverId)
    {
        return await context.DriveLicenses
            .Where(l => l.DriverId == driverId)
            .ToListAsync();
    }
}