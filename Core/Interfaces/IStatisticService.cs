using WaybillWpf.Core.DTO;

namespace WaybillWpf.Core.Interfaces;

public interface IStatisticService
{
    Task<DashboardKpi> GetDashboardKpisAsync(DateTime startDate, DateTime endDate);

    // 2. Получить отчет "План/Факт" по расходу топлива (по каждому листу)
    Task<ICollection<FuelEfficiencyReportItem>> GetFuelEfficiencyReportAsync(DateTime startDate, DateTime endDate);
        
    // 3. Получить суммарный отчет по пробегу, сгруппированный по машинам
    Task<ICollection<MileageReportItem>> GetMileageByCarAsync(DateTime startDate, DateTime endDate);
        
    // 4. Получить суммарный отчет по пробегу, сгруппированный по водителям
    Task<ICollection<MileageReportItem>> GetMileageByDriverAsync(DateTime startDate, DateTime endDate);
}