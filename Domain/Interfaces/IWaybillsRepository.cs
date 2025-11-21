using WaybillWpf.Domain.Entities;

namespace WaybillWpf.Domain.Interfaces;

public interface IWaybillsRepository: IBaseRepository<Waybill>
{
    public Task<ICollection<Waybill>> GetWaybillsByDateRangeAsync(DateTime startDate, DateTime endDate);
    public Task<ICollection<Waybill>> GetWaybillsByUserIdAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    public Task<ICollection<Waybill>> GetWaybillsByUserIdAsync(int userId);
    
}