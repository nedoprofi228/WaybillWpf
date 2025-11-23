using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class WaybillTaskRepository(ApplicationContext context): BaseRepository<WaybillTask>(context), IWaybillTasksRepository
{
    
}