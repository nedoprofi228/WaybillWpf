using WaybillWpf.DataBase;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Exceptions;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services;

public class WaybillDetailsManagement(
    IWaybillDetailsRepository waybillDetailsRepository,
    IWaybillsRepository waybillsRepository) : IWaybillsDetailsManagement
{
    public async Task<WaybillDetails> AddDetailAsync(WaybillDetails detail)
    {
        await waybillDetailsRepository.AddAsync(detail);
        return detail;
    }

    public async Task<bool> UpdateDetailAsync(WaybillDetails detail)
    {
        return await waybillDetailsRepository.UpdateAsync(detail);
    }

    public async Task<bool> DeleteDetailAsync(int detailId)
    {
        var detail = await waybillDetailsRepository.GetByIdAsync(detailId);
        if (detail == null) return false;

        // БИЗНЕС-ПРАВИЛО: Нельзя удалять детали из уже выданного листа
        var waybill = await waybillsRepository.GetByIdAsync(detail.WaybillId);
        if (waybill != null && waybill.WaybillStatus == WaybillStatus.InProgress )
        {
            throw new WaybillValidationException("Удалять отрезки пути можно только у листа который находится в в процессе");
        }

        return await waybillDetailsRepository.DeleteAsync(detail);
    }

}

