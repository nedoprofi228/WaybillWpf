using Microsoft.EntityFrameworkCore;
using WaybillWpf.Core.Entities;
using WaybillWpf.Core.Interfaces;

namespace WaybillWpf.DataBase;

public class WaybillDetailsRepository(ApplicationContext context) : BaseRepository<WaybillDetails>(context), IWaybillDetailsRepository
{

}