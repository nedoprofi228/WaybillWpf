// В .Domain/Interfaces/IWaybillFlowService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;

namespace WaybillWpf.Domain.Interfaces
{
    /// <summary>
    /// Сервис, управляющий жизненным циклом (потоком) путевого листа.
    /// </summary>
    public interface IWaybillFlowService
    {
        // 1. Создать "шапку" путевого листа (статус "Draft")
        Task<Waybill> CreateDraftWaybillAsync(Logist user, Car car, Driver driver);

        // 2. Получить полный лист со всеми "плечами" (Details) для редактирования
        Task<Waybill?> GetWaybillByIdAsync(int waybillId);
        
        // 8. Получить список листов для Админа/Менеджера
        Task<ICollection<Waybill>> GetWaybillsAsync(int? managerId, WaybillStatus? status);
        
        Task<WaybillTask> AddTaskAsync(WaybillTask task);
    
        Task<bool> UpdateTaskAsync(WaybillTask task);
    
        Task<bool> DeleteTaskAsync(int taskId);
        public Task<bool> DeleteWaybillAsync(int waybillId);
    }
}