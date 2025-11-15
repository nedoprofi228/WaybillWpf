using WaybillWpf.Core.Entities;
using WaybillWpf.Core.Interfaces;

namespace WaybillWpf.DataBase;

public class CarRepository(ApplicationContext context) : BaseRepository<Car>(context), ICarsRepository
{
    
}