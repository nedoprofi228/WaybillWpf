// В .Services/CarManagementService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaybillWpf.Domain.DTO;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services
{
    public class CarManagementService(ICarsRepository carsRepository, IWaybillsRepository waybillsRepository) : ICarManagementService
    {

        
        public async Task<ICollection<Car>> GetAllCarsAsync()
        {
            return await carsRepository.GetAllAsync();
        }
        
        public async Task<Car?> GetCarByIdAsync(int carId)
        {
            return await carsRepository.GetByIdAsync(carId);
        }
        
        public async Task<bool> SaveCarAsync(Car car)
        {
            if (car == null) return false;

            if (car.Id == 0) // Новая машина
            {
                return await carsRepository.AddAsync(car);
            }
            else // Существующая машина
            {
                return await carsRepository.UpdateAsync(car);
            }
        }
        
        public async Task<ICollection<Car>> GetAvailableCarsAsync()
        {
            var allCars = await carsRepository.GetAllAsync();
            var allWaybills = await waybillsRepository.GetAllAsync();

            // 1. Находим ID всех машин, которые сейчас "Выданы" (на рейсе)
            var unavailableCarIds = allWaybills
                .Where(w => w.WaybillStatus == WaybillStatus.InProgress)
                .Select(w => w.CarId)
                .ToHashSet(); // ToHashSet() - для быстрой проверки

            // 2. Возвращаем только те машины, которых нет в этом списке
            var availableCars = allCars
                .Where(c => !unavailableCarIds.Contains(c.Id))
                .ToList();

            return availableCars;
        }
        
        public async Task<bool> DeleteCarAsync(int carId)
        {
            var allWaybills = await waybillsRepository.GetAllAsync();

            // 1. Проверяем бизнес-логику: не на активном рейсе
            bool hasActiveWaybills = allWaybills.Any(w => w.CarId == carId &&
                                                          w.WaybillStatus == WaybillStatus.InProgress);
            if (hasActiveWaybills)
            {
                throw new Exception("Нельзя удалить машину, она находится на активном рейсе.");
            }
            
            var car = await carsRepository.GetByIdAsync(carId);
            if (car == null)
            {
                throw new Exception("Машина не найдена.");
            }
            
            bool deleteSuccess = await carsRepository.DeleteAsync(car);
            
            if (!deleteSuccess)
            {
                 throw new Exception("Произошла ошибка при удалении.");
            }
            
            return true;
        }
    }
}