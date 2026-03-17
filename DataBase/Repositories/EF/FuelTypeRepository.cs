using WaybillWpf.DataBase;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase.Repositories.EF
{
    public class FuelTypeRepository(ApplicationContext context) : BaseRepository<FuelType>(context), IFuelTypesRepository
    {
    }
}
