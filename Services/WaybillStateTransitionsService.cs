using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Exceptions;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services;

public class WaybillStateTransitionsService(IWaybillsRepository waybillsRepository, ICurrentUserService currentUserService) : IWaybillStateTransitionsService
{
    public async Task<bool> IssueWaybillAsync(int waybillId)
    {
        if(!currentUserService.IsManager) throw new UnauthorizedAccessException("Издавать путевые листы может только логист");
        
        var waybill = await waybillsRepository.GetByIdAsync(waybillId);
            
        // Проверка 1: Существование
        if (waybill == null)
            throw new WaybillFlowException($"Путевой лист с Id={waybillId} не найден.");
            
        // Проверка 2: Статус
        if (waybill.WaybillStatus != WaybillStatus.Draft)
            throw new WaybillValidationException("Выдать можно только путевой лист в статусе 'Черновик'.");

        // Все проверки пройдены
        waybill.WaybillStatus = WaybillStatus.InProgress;
        await waybillsRepository.UpdateAsync(waybill);
        return true;
    }

    public async Task<bool> CompleteWaybillAsync(int waybillId)
    {
        if(!currentUserService.IsManager) throw new UnauthorizedAccessException("Подтверждать выполнение путевого листа может только логист");
        
        var waybill = await waybillsRepository.GetByIdAsync(waybillId);
        if (waybill == null) throw new Exception("Путевой лист не найден");

        // Проверки
        if (waybill.WaybillStatus != WaybillStatus.Accepting)
        {
            throw new Exception("Завершить можно только лист, находящийся в ожидании подтверждения.");
        }
            
        // Проверка 3: Наличие "плеч" (деталей)
        if (!waybill.WaybillDetails.Any())
            throw new WaybillValidationException("Нельзя выдать пустой путевой лист. Добавьте хотя бы один отрезок пути.");

        // Проверка 4: Заполненность "плеч" (данные по выезду)
        foreach (var detail in waybill.WaybillDetails)
        { 
            if (detail.DepartureDateTime == DateTime.MinValue || detail.StartMealing < 0 || detail.StartRemeaningFuel < 0)
                throw new WaybillValidationException("Все отрезки пути должны иметь дату выезда, начальный пробег и начальный остаток топлива.");
        }
 
        waybill.WaybillStatus = WaybillStatus.Completed;
        await waybillsRepository.UpdateAsync(waybill);
        return true;
    }
    
    public async Task<bool> ArchiveWaybillAsync(int waybillId)
    {
        if (!currentUserService.IsManager)
            throw new UnauthorizedAccessException("Архивировать путевой листы может только логист");
        var waybill = await waybillsRepository.GetByIdAsync(waybillId);
        if (waybill == null) throw new Exception("Путевой лист не найден");

        if (waybill.WaybillStatus != WaybillStatus.Completed)
        {
            throw new Exception("В архив можно сдать только завершенный (Completed) путевой лист.");
        }

        waybill.WaybillStatus = WaybillStatus.Archived;
        await waybillsRepository.UpdateAsync(waybill);
        return true;
    }

    public async Task<bool> AcceptingWaybillAsync(int waybillId)
    {
        if (!currentUserService.IsDriver)
            throw new UnauthorizedAccessException("Отправить на подтверждение может только водитель");
        
        var waybill = await waybillsRepository.GetByIdAsync(waybillId);
        if (waybill == null) throw new Exception("Путевой лист не найден");

        if (waybill.WaybillStatus != WaybillStatus.InProgress)
        {
            throw new Exception("Подтвердить выполнение можно только у путевого листа который находиться в процессе выполнения");
        }

        waybill.WaybillStatus = WaybillStatus.Accepting;
        await waybillsRepository.UpdateAsync(waybill);
        return true;
    }

    public async Task<bool> DeclineWaybillAsync(int waybillId, string reason)
    {
        if(!currentUserService.IsManager) throw new UnauthorizedAccessException("Подтверждать выполнение путевого листа может только логист");
        
        var waybill = await waybillsRepository.GetByIdAsync(waybillId);
        if (waybill == null) throw new Exception("Путевой лист не найден");

        // Проверки
        if (waybill.WaybillStatus != WaybillStatus.Accepting)
        {
            throw new Exception("Завершить можно только лист, находящийся в ожидании подтверждения.");
        }
 
        waybill.WaybillStatus = WaybillStatus.Declinede;
        waybill.ReasonOfDecline = reason;
        await waybillsRepository.UpdateAsync(waybill);
        return true;
    }
}