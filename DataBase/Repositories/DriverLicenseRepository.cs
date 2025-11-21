using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

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