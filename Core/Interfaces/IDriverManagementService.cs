using WaybillWpf.Core.DTO;
using WaybillWpf.Core.Entities;

namespace WaybillWpf.Services;

// В .Core/Interfaces/
public interface IDriverManagementService
{

    // Получить всех водителей с их ВУ
    Task<ICollection<DriverDto>> GetAllDriversAsync();
    
    // Получить полную модель для редактирования
    Task<Driver> GetDriverForEditAsync(int driverId); // Включить .Include(d => d.DriveLicense)
    
    // Сохранить (создать или обновить) водителя и его ВУ
    Task<bool> SaveDriverAsync(Driver driver); // EF сам разберется с DriveLicense, если он вложен
    
    // Получить водителей, которые СВОБОДНЫ (не в "Issued" рейсе)
    Task<ICollection<Driver>> GetAvailableDriversAsync();
    
    Task<bool> DeleteDriverAsync(int driverId); // (с проверкой, что он не на рейсе)
}