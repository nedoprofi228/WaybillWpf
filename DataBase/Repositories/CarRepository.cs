using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class CarRepository(ApplicationContext context) : BaseRepository<Car>(context), ICarsRepository
{
    
}