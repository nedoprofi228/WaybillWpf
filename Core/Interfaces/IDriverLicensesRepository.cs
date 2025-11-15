using WaybillWpf.Core.Entities;

namespace WaybillWpf.Core.Interfaces;

public interface IDriverLicensesRepository: IBaseRepository<DriveLicense>
{
    public Task<ICollection<DriveLicense>> GetAllLicensesBydriverIdAsync(int driverId);
}