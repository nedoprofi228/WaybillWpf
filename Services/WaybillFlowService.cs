// В .Services/WaybillFlowService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Exceptions;
using WaybillWpf.Domain.Interfaces;

namespace WaybillWpf.Services
{
    public class WaybillFlowService(IWaybillTasksRepository waybillTasksRepo, IWaybillsRepository waybillsRepo) : IWaybillFlowService
    {

        public async Task<Waybill> CreateDraftWaybillAsync(Logist user, Car car, Driver driver)
        {
            var waybill = new Waybill
            {
                Logist = user,
                CarId = car.Id,
                DriverId = driver.Id,
                WaybillStatus = WaybillStatus.Draft // Статус по умолчанию
            };

            await waybillsRepo.AddAsync(waybill);

            waybill.Car = car;
            waybill.Driver = driver;
            
            return waybill; // Возвращаем сущность с Id
        }

        public async Task<Waybill?> GetWaybillByIdAsync(int waybillId)
        {
            return await waybillsRepo.GetByIdAsync(waybillId);
        }
        
        
        public async Task<ICollection<Waybill>> GetWaybillsAsync(int? managerId, WaybillStatus? status)
        {
            // Используем более эффективные методы репозитория, если они есть
            if (managerId.HasValue && !status.HasValue)
            {
                return await waybillsRepo.GetWaybillsByUserIdAsync(managerId.Value);
            }

            // Общий случай с фильтрацией
            var allWaybills = await waybillsRepo.GetAllAsync();

            if (managerId.HasValue)
            {
                allWaybills = allWaybills.Where(w => w.LogistId == managerId.Value).ToList();
            }

            if (status.HasValue)
            {
                allWaybills = allWaybills.Where(w => w.WaybillStatus == status.Value).ToList();
            }

            return allWaybills;
        }
        
        public async Task<WaybillTask> AddTaskAsync(WaybillTask task)
        {
            if (task.Waybill.WaybillStatus == WaybillStatus.Archived)
            {
                    throw new Exception("Нельзя менять архивный лист.");
            }

            await waybillTasksRepo.AddAsync(task); // _tasksRepository - это инъекция IWaybillTasksRepository
            return task;
        }

        public async Task<bool> UpdateTaskAsync(WaybillTask task)
        {
            // Аналогичные проверки...
            return await waybillTasksRepo.UpdateAsync(task);
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await waybillTasksRepo.GetByIdAsync(taskId);
            if (task == null) return false;
    
            return await waybillTasksRepo.DeleteAsync(task);
        }

        public async Task<bool> DeleteWaybillAsync(int waybillId)
        {
            var waybill = await waybillsRepo.GetByIdAsync(waybillId);
            if (waybill == null) return false;
            
            return await waybillsRepo.DeleteAsync(waybill);
        }
    }
}