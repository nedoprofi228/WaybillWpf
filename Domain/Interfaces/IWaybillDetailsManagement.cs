using WaybillWpf.Domain.Entities;

namespace WaybillWpf.Domain.Interfaces;

public interface IWaybillsDetailsManagement
{
    Task<WaybillDetails> AddDetailAsync(WaybillDetails detail);
    Task<bool> UpdateDetailAsync(WaybillDetails detail);
    Task<bool> DeleteDetailAsync(int detailId);
}