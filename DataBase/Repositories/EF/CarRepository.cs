using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class CarRepository(ApplicationContext context) : BaseRepository<Car>(context), ICarsRepository
{
    public override async Task<ICollection<Car>> GetAllAsync()
    {
        return await context.Cars.AsNoTracking().Include(c => c.FuelType).ToListAsync();
    }
}