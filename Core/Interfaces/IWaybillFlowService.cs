// В .Core/Interfaces/IWaybillFlowService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using WaybillWpf.Core.Entities;
using WaybillWpf.Core.Enums;

namespace WaybillWpf.Core.Interfaces
{
    /// <summary>
    /// Сервис, управляющий жизненным циклом (потоком) путевого листа.
    /// </summary>
    public interface IWaybillFlowService
    {
        // 1. Создать "шапку" путевого листа (статус "Draft")
        Task<Waybill> CreateDraftWaybillAsync(int managerId, int carId, int driverId);

        // 2. Получить полный лист со всеми "плечами" (Details) для редактирования
        Task<Waybill?> GetWaybillForEditAsync(int waybillId);

        // --- Управление "плечами" (WaybillDetails) ---

        // 3. Добавить "плечо" (отрезок пути) к черновику
        Task<WaybillDetails> AddDetailAsync(WaybillDetails detail);
        
        // 4. Обновить "плечо"
        Task<bool> UpdateDetailAsync(WaybillDetails detail);

        // 5. Удалить "плечо" (вернет false, если не найдено)
        // Выбросит WaybillValidationException, если лист не в статусе "Draft"
        Task<bool> DeleteDetailAsync(int detailId);
        
        // --- Управление статусами (Бизнес-логика) ---

        // 6. "Выдать" лист (перевод в "Issued").
        // Выбросит WaybillValidationException, если не выполнены бизнес-правила
        Task IssueWaybillAsync(int waybillId);

        // 7. "Завершить" лист (перевод в "Completed").
        // Выбросит WaybillValidationException, если не выполнены бизнес-правила
        Task CompleteWaybillAsync(int waybillId);
        
        // 8. Получить список листов для Админа/Менеджера
        Task<ICollection<Waybill>> GetWaybillsAsync(int? managerId, WaybillStatus? status);
    }
}