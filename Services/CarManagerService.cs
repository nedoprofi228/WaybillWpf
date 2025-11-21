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
    public class CarManagementService : ICarManagementService
    {
        private readonly ICarsRepository _carsRepo;
        private readonly IWaybillsRepository _waybillsRepo;

        public CarManagementService()
        {
            // Получаем зависимости из вашего DI-провайдера
            _carsRepo = ServicesProvider.GetService<ICarsRepository>()
                        ?? throw new InvalidOperationException("ICarsRepository is not registered.");
            _waybillsRepo = ServicesProvider.GetService<IWaybillsRepository>()
                            ?? throw new InvalidOperationException("IWaybillsRepository is not registered.");
        }

        /// <summary>
        /// Получить все машины.
        /// </summary>
        public async Task<ICollection<Car>> GetAllCarsAsync()
        {
            return await _carsRepo.GetAllAsync();
        }

        /// <summary>
        /// Получить одну машину по Id.
        /// </summary>
        public async Task<Car?> GetCarByIdAsync(int carId)
        {
            return await _carsRepo.GetByIdAsync(carId);
        }

        /// <summary>
        /// Сохранить машину (создать новую или обновить существующую).
        /// </summary>
        public async Task<bool> SaveCarAsync(Car car)
        {
            if (car == null) return false;

            if (car.Id == 0) // Новая машина
            {
                return await _carsRepo.AddAsync(car);
            }
            else // Существующая машина
            {
                return await _carsRepo.UpdateAsync(car);
            }
        }

        /// <summary>
        /// Получить список машин, которые СВОБОДНЫ (не на "Issued" рейсе).
        /// </summary>
        public async Task<ICollection<Car>> GetAvailableCarsAsync()
        {
            var allCars = await _carsRepo.GetAllAsync();
            var allWaybills = await _waybillsRepo.GetAllAsync();

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

        /// <summary>
        /// Удалить машину (с проверкой, что она не на рейсе).
        /// </summary>
        public async Task<bool> DeleteCarAsync(int carId)
        {
            var allWaybills = await _waybillsRepo.GetAllAsync();

            // 1. Проверяем бизнес-логику: не на активном рейсе
            bool hasActiveWaybills = allWaybills.Any(w => w.CarId == carId &&
                                                          w.WaybillStatus == WaybillStatus.InProgress);
            if (hasActiveWaybills)
            {
                throw new Exception("Нельзя удалить машину, она находится на активном рейсе.");
            }

            // 2. Проверяем, есть ли у машины *вообще* путевые листы (даже завершенные)
            // В зависимости от требований, вы можете запретить удаление, если есть история.
            // Для примера, разрешим (удаление возможно, если нет АКТИВНЫХ).
            // bool hasHistory = allWaybills.Any(w => w.CarId == carId);
            // if (hasHistory) ...
            
            // 3. Находим машину
            var car = await _carsRepo.GetByIdAsync(carId);
            if (car == null)
            {
                throw new Exception("Машина не найдена.");
            }

            // 4. Удаляем машину
            bool deleteSuccess = await _carsRepo.DeleteAsync(car);
            
            if (!deleteSuccess)
            {
                 throw new Exception("Произошла ошибка при удалении.");
            }
            
            return true;
        }
    }
}