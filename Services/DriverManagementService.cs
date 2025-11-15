// В .Services/DriverManagementService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaybillWpf.Core.DTO;
using WaybillWpf.Core.Entities;
using WaybillWpf.Core.Enums;
using WaybillWpf.Core.Interfaces;

namespace WaybillWpf.Services
{
    public class DriverManagementService : IDriverManagementService
    {
        private readonly IDriversRepository _driversRepo;
        private readonly IDriverLicensesRepository _licensesRepo;
        private readonly IWaybillsRepository _waybillsRepo;

        public DriverManagementService()
        {
            // Получаем зависимости из вашего DI-провайдера
            _driversRepo = ServicesProvider.GetService<IDriversRepository>()
                           ?? throw new InvalidOperationException("IDriversRepository is not registered.");
            _licensesRepo = ServicesProvider.GetService<IDriverLicensesRepository>()
                            ?? throw new InvalidOperationException("IDriverLicensesRepository is not registered.");
            _waybillsRepo = ServicesProvider.GetService<IWaybillsRepository>()
                            ?? throw new InvalidOperationException("IWaybillsRepository is not registered.");
        }

        /// <summary>
        /// Получить всех водителей с данными их ВУ для отображения в списке.
        /// </summary>
        public async Task<ICollection<DriverDto>> GetAllDriversAsync()
        {
            var drivers = await _driversRepo.GetAllAsync();
            var licenses = await _licensesRepo.GetAllAsync();

            // Соединяем в памяти водителей и их ВУ
            var dtos = drivers.Select(d =>
            {
                var license = licenses.FirstOrDefault(l => l.DriverId == d.Id);
                return new DriverDto
                {
                    DriverId = d.Id,
                    DriverName = d.DriverName,
                    LicenseNumber = license?.LicenseNumber ?? "N/A",
                    LicenseExpiration = license?.ExpirationDate ?? DateTime.MinValue
                };
            }).ToList();

            return dtos;
        }

        /// <summary>
        /// Получить полную сущность водителя (включая ВУ) для редактирования.
        /// </summary>
        public async Task<Driver?> GetDriverForEditAsync(int driverId)
        {
            var driver = await _driversRepo.GetByIdAsync(driverId);
            if (driver == null) return null;

            // Вручную "подтягиваем" связанную 1-к-1 сущность
            driver.DriveLicense = (await _licensesRepo.GetAllLicensesBydriverIdAsync(driverId)).FirstOrDefault();
            
            return driver;
        }

        /// <summary>
        /// Сохранить водителя (нового или существующего) и его ВУ.
        /// </summary>
        public async Task<bool> SaveDriverAsync(Driver driver)
        {
            if (driver == null) return false;

            if (driver.Id == 0) // Новый водитель
            {
                // AddAsync(driver) должен добавить и driver, и связанный driver.DriveLicense
                // (если EF Core настроен правильно)
                return await _driversRepo.AddAsync(driver);
            }
            else // Существующий водитель
            {
                // 1. Обновляем самого водителя
                await _driversRepo.UpdateAsync(driver);

                // 2. Явно обновляем/добавляем ВУ
                if (driver.DriveLicense != null)
                {
                    var existingLicense = (await _licensesRepo.GetAllLicensesBydriverIdAsync(driver.Id)).FirstOrDefault();
                    
                    if (existingLicense == null) // У водителя не было ВУ, добавляем
                    {
                        driver.DriveLicense.DriverId = driver.Id; // Устанавливаем FK
                        await _licensesRepo.AddAsync(driver.DriveLicense);
                    }
                    else // У водителя было ВУ, обновляем
                    {
                        // Переносим данные из VM в отслеживаемую сущность
                        existingLicense.LicenseNumber = driver.DriveLicense.LicenseNumber;
                        existingLicense.IssueDate = driver.DriveLicense.IssueDate;
                        existingLicense.ExpirationDate = driver.DriveLicense.ExpirationDate;
                        await _licensesRepo.UpdateAsync(existingLicense);
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Получить список водителей, которые СВОБОДНЫ (не на "Issued" рейсе).
        /// </summary>
        public async Task<ICollection<Driver>> GetAvailableDriversAsync()
        {
            var allDrivers = await _driversRepo.GetAllAsync();
            var allWaybills = await _waybillsRepo.GetAllAsync();

            // 1. Находим ID всех водителей, которые сейчас "Выданы" (на рейсе)
            var unavailableDriverIds = allWaybills
                .Where(w => w.WaybillStatus == WaybillStatus.Draft)
                .Select(w => w.DriverId)
                .ToHashSet(); // ToHashSet() - для быстрой проверки

            // 2. Возвращаем только тех водителей, которых нет в этом списке
            var availableDrivers = allDrivers
                .Where(d => !unavailableDriverIds.Contains(d.Id))
                .ToList();

            return availableDrivers;
        }

        /// <summary>
        /// Удалить водителя и его ВУ (с проверкой, что он не на рейсе).
        /// </summary>
        public async Task<bool> DeleteDriverAsync(int driverId)
        {
            var allWaybills = await _waybillsRepo.GetAllAsync();

            // 1. Проверяем бизнес-логику: не на активном рейсе
            bool hasActiveWaybills = allWaybills.Any(w => w.DriverId == driverId && 
                                                          w.WaybillStatus == WaybillStatus.Draft);
            if (hasActiveWaybills)
            {
                throw new Exception("Нельзя удалить водителя, он находится на активном рейсе.");
            }

            // 2. Находим водителя
            var driver = await _driversRepo.GetByIdAsync(driverId);
            if (driver == null)
            {
                throw new Exception("Водитель не найден.");
            }

            // 3. Находим и удаляем связанное ВУ (если оно есть)
            var license = (await _licensesRepo.GetAllLicensesBydriverIdAsync(driverId)).FirstOrDefault();
            if (license != null)
            {
                await _licensesRepo.DeleteAsync(license);
            }
            
            // 4. Удаляем самого водителя
            await _driversRepo.DeleteAsync(driver);

            return true;
        }
    }
}