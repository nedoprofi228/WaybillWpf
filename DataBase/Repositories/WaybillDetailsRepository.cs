using Microsoft.EntityFrameworkCore;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.DataBase;

public class WaybillDetailsRepository(ApplicationContext context) : BaseRepository<WaybillDetails>(context), IWaybillDetailsRepository
{

}