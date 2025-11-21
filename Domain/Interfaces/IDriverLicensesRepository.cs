using WaybillWpf.Domain.Entities;

namespace WaybillWpf.Domain.Interfaces;

public interface IDriverLicensesRepository: IBaseRepository<DriveLicense>
{
    public Task<ICollection<DriveLicense>> GetAllLicensesBydriverIdAsync(int driverId);
}