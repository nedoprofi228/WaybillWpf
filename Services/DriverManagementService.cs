// В .Services/DriverManagementService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaybillWpf.Domain.DTO;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services
{
    public class DriverManagementService(IDriversRepository driversRepository, IDriverLicensesRepository licensesRepository, IWaybillsRepository waybillsRepository) : IDriverManagementService
    {
        
        public async Task<ICollection<DriverDto>> GetAllDriversAsync()
        {
            var drivers = await driversRepository.GetAllAsync();
            var dtos = drivers.Select(d =>
            {
                return new DriverDto
                {
                    DriverId = d.Id,
                    DriverName = d.DriverName,
                    LicenseNumber = d.DriveLicense?.LicenseNumber ?? "N/A",
                    LicenseExpiration =  d.DriveLicense?.ExpirationDate ?? DateTime.MinValue
                };
            }).ToList();

            return dtos;
        }

        public Task<Driver> GetDriverById(int driverId)
        {
            return driversRepository.GetByIdAsync(driverId);
        }
        
        public async Task<bool> SaveDriverAsync(Driver driver)
        {
            if (driver == null) return false;

            if (driver.Id == 0) 
            {
                return await driversRepository.AddAsync(driver);
            }
            else
            {
                await driversRepository.UpdateAsync(driver);
                
                if (driver.DriveLicense != null)
                {
                    var existingLicense = await licensesRepository.GetByIdAsync(driver.DriverLicenseId);
                    
                    if (existingLicense == null)
                    {
                        await licensesRepository.AddAsync(driver.DriveLicense);
                    }
                    else
                    {
                        existingLicense.LicenseNumber = driver.DriveLicense.LicenseNumber;
                        existingLicense.IssueDate = driver.DriveLicense.IssueDate;
                        existingLicense.ExpirationDate = driver.DriveLicense.ExpirationDate;
                        await licensesRepository.UpdateAsync(existingLicense);
                    }
                }
                return true;
            }
        }
        
        public async Task<ICollection<Driver>> GetAvailableDriversAsync()
        {
            var allDrivers = await driversRepository.GetAllAsync();
            var allWaybills = await waybillsRepository.GetAllAsync();
            
            var unavailableDriverIds = allWaybills
                .Where(w => w.WaybillStatus == WaybillStatus.InProgress)
                .Select(w => w.DriverId)
                .ToHashSet(); // ToHashSet() - для быстрой проверки
            
            var availableDrivers = allDrivers
                .Where(d => !unavailableDriverIds.Contains(d.Id))
                .ToList();

            return availableDrivers;
        }
        
        public async Task<bool> DeleteDriverAsync(int driverId)
        {
            var allWaybills = await waybillsRepository.GetAllAsync();
            
            bool hasActiveWaybills = allWaybills.Any(w => w.DriverId == driverId && 
                                                          w.WaybillStatus != WaybillStatus.InProgress);
            if (hasActiveWaybills)
            {
                throw new Exception("Нельзя удалить водителя, он находится на активном рейсе.");
            }
            
            var driver = await driversRepository.GetByIdAsync(driverId);
            if (driver == null)
            {
                throw new Exception("Водитель не найден.");
            }
            
            var license = (await licensesRepository.GetByIdAsync(driver.DriverLicenseId));
            if (license != null)
            {
                await licensesRepository.DeleteAsync(license);
            }
            
            await driversRepository.DeleteAsync(driver);

            return true;
        }
    }
}