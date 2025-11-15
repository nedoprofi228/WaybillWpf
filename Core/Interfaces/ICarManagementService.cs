using WaybillWpf.Core.Entities;

namespace WaybillWpf.Core.Interfaces;

// В .Core/Interfaces/
public interface ICarManagementService
{
    Task<ICollection<Car>> GetAllCarsAsync();
    Task<Car> GetCarByIdAsync(int carId);
    Task<bool> SaveCarAsync(Car car); // (create/update)
    Task<ICollection<Car>> GetAvailableCarsAsync(); // (не в "Issued" рейсе)
    Task<bool> DeleteCarAsync(int carId); // (с проверкой)
} 