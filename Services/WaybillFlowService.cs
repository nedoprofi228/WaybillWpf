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
    public class WaybillFlowService : IWaybillFlowService
    {
        private readonly IWaybillsRepository _waybillsRepo;
        private readonly IWaybillDetailsRepository _detailsRepo;
        private readonly IWaybillTasksRepository _waybillTasksRepo;

        public WaybillFlowService(IWaybillTasksRepository waybillTasksRepo, IWaybillsRepository waybillsRepo, IWaybillDetailsRepository detailsRepo)
        {
            _waybillsRepo = waybillsRepo;
            _detailsRepo = detailsRepo;
            _waybillTasksRepo = waybillTasksRepo;
        }

        public async Task<Waybill> CreateDraftWaybillAsync(int managerId, int carId, int driverId)
        {
            var waybill = new Waybill
            {
                UserId = managerId,
                CarId = carId,
                DriverId = driverId,
                WaybillStatus = WaybillStatus.Draft // Статус по умолчанию
            };

            await _waybillsRepo.AddAsync(waybill);
            return waybill; // Возвращаем сущность с Id
        }

        public async Task<Waybill?> GetWaybillByIdAsync(int waybillId)
        {
            return await _waybillsRepo.GetByIdAsync(waybillId);
        }

        #region WaybillDetails Management

        public async Task<WaybillDetails> AddDetailAsync(WaybillDetails detail)
        {
            await _detailsRepo.AddAsync(detail);
            return detail;
        }

        public async Task<bool> UpdateDetailAsync(WaybillDetails detail)
        {
            return await _detailsRepo.UpdateAsync(detail);
        }

        public async Task<bool> DeleteDetailAsync(int detailId)
        {
            var detail = await _detailsRepo.GetByIdAsync(detailId);
            if (detail == null) return false;

            // БИЗНЕС-ПРАВИЛО: Нельзя удалять детали из уже выданного листа
            var waybill = await _waybillsRepo.GetByIdAsync(detail.WaybillId);
            if (waybill != null && waybill.WaybillStatus != WaybillStatus.Draft)
            {
                throw new WaybillValidationException("Нельзя удалять отрезки пути из путевого листа, который уже выдан или завершен.");
            }

            return await _detailsRepo.DeleteAsync(detail);
        }

        #endregion

        #region State Transitions (Business Logic)

        public async Task IssueWaybillAsync(int waybillId)
        {
            var waybill = await GetWaybillByIdAsync(waybillId);
            
            // Проверка 1: Существование
            if (waybill == null)
                throw new WaybillFlowException($"Путевой лист с Id={waybillId} не найден.");
            
            // Проверка 2: Статус
            if (waybill.WaybillStatus != WaybillStatus.Draft)
                throw new WaybillValidationException("Выдать можно только путевой лист в статусе 'Черновик'.");

            // Все проверки пройдены
            waybill.WaybillStatus = WaybillStatus.InProgress;
            await _waybillsRepo.UpdateAsync(waybill);
        }

        public async Task CompleteWaybillAsync(int waybillId)
        {
            var waybill = await _waybillsRepo.GetByIdAsync(waybillId);
            if (waybill == null) throw new Exception("Путевой лист не найден");

            // Проверки
            if (waybill.WaybillStatus != WaybillStatus.InProgress)
            {
                throw new Exception("Завершить можно только лист, находящийся в работе (InProgress).");
            }
            
            // Проверка 3: Наличие "плеч" (деталей)
            if (!waybill.WaybillDetails.Any())
                throw new WaybillValidationException("Нельзя выдать пустой путевой лист. Добавьте хотя бы один отрезок пути.");

            // Проверка 4: Заполненность "плеч" (данные по выезду)
            foreach (var detail in waybill.WaybillDetails)
            { 
                if (detail.DepartureDateTime == DateTime.MinValue || detail.StartMealing <= 0 || detail.StartRemeaningFuel <= 0)
                    throw new WaybillValidationException("Все отрезки пути должны иметь дату выезда, начальный пробег и начальный остаток топлива.");
            }
 
            waybill.WaybillStatus = WaybillStatus.Completed;
            await _waybillsRepo.UpdateAsync(waybill);
        }
        
        #endregion
        
        public async Task ArchiveWaybillAsync(int waybillId)
        {
            var waybill = await _waybillsRepo.GetByIdAsync(waybillId);
            if (waybill == null) throw new Exception("Путевой лист не найден");

            if (waybill.WaybillStatus != WaybillStatus.Completed)
            {
                throw new Exception("В архив можно сдать только завершенный (Completed) путевой лист.");
            }

            waybill.WaybillStatus = WaybillStatus.Archived;
            await _waybillsRepo.UpdateAsync(waybill);
        }
        
        public async Task<ICollection<Waybill>> GetWaybillsAsync(int? managerId, WaybillStatus? status)
        {
            // Используем более эффективные методы репозитория, если они есть
            if (managerId.HasValue && !status.HasValue)
            {
                return await _waybillsRepo.GetWaybillsByUserIdAsync(managerId.Value);
            }

            // Общий случай с фильтрацией
            var allWaybills = await _waybillsRepo.GetAllAsync();

            if (managerId.HasValue)
            {
                allWaybills = allWaybills.Where(w => w.UserId == managerId.Value).ToList();
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

            await _waybillTasksRepo.AddAsync(task); // _tasksRepository - это инъекция IWaybillTasksRepository
            return task;
        }

        public async Task<bool> UpdateTaskAsync(WaybillTask task)
        {
            // Аналогичные проверки...
            return await _waybillTasksRepo.UpdateAsync(task);
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = await _waybillTasksRepo.GetByIdAsync(taskId);
            if (task == null) return false;
    
            return await _waybillTasksRepo.DeleteAsync(task);
        }

        public async Task<bool> DeleteWaybillAsync(int waybillId)
        {
            var waybill = await _waybillsRepo.GetByIdAsync(waybillId);
            if (waybill == null) return false;
            
            return await _waybillsRepo.DeleteAsync(waybill);
        }
    }
}