using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Interfaces;

public interface IWaybillsRepository: IBaseRepository<Waybill>
{
    public Task<ICollection<Waybill>> GetWaybillsByDateRangeAsync(DateTime startDate, DateTime endDate);
    public Task<ICollection<Waybill>> GetWaybillsByUserIdAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    public Task<ICollection<Waybill>> GetWaybillsByUserIdAsync(int userId);
    public Task<ICollection<Waybill>> GetWaybillsByDriverIdAsync(int userId);
    
    
}