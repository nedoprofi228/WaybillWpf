namespace WaybillWpf.Domain.Interfaces;

public interface IWaybillStateTransitionsService
{
    // 6. "Выдать" лист (перевод в "Issued").
    // Выбросит WaybillValidationException, если не выполнены бизнес-правила
    Task<bool> IssueWaybillAsync(int waybillId);

    // 7. "Завершить" лист (перевод в "Completed").
    // Выбросит WaybillValidationException, если не выполнены бизнес-правила
    Task<bool> CompleteWaybillAsync(int waybillId);

    Task<bool> ArchiveWaybillAsync(int waybillId);
    
    Task<bool> AcceptingWaybillAsync(int waybillId);
    Task<bool> DeclineWaybillAsync(int waybillId, string reason);
}