
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
    public class StatisticService : IStatisticService
    {
        // Репозитории для доступа к данным
        private readonly IWaybillsRepository _waybillsRepo;
        private readonly IWaybillDetailsRepository _detailsRepo;
        private readonly ICarsRepository _carsRepo;
        private readonly IDriversRepository _driversRepo;

        // Конструктор получает все зависимости через ваш ServicesProvider
        public StatisticService()
        {
            _waybillsRepo = ServicesProvider.GetService<IWaybillsRepository>()
                            ?? throw new InvalidOperationException("IWaybillsRepository is not registered.");
            
            _detailsRepo = ServicesProvider.GetService<IWaybillDetailsRepository>()
                           ?? throw new InvalidOperationException("IWaybillDetailsRepository is not registered.");
            
            _carsRepo = ServicesProvider.GetService<ICarsRepository>()
                        ?? throw new InvalidOperationException("ICarsRepository is not registered.");
            
            _driversRepo = ServicesProvider.GetService<IDriversRepository>()
                           ?? throw new InvalidOperationException("IDriversRepository is not registered.");
        }

        #region Public Methods (IStatisticService Implementation)

        /// <summary>
        /// 1. Получает главные KPI (цифры) для дашборда админа.
        /// </summary>
        public async Task<DashboardKpi> GetDashboardKpisAsync(DateTime startDate, DateTime endDate)
        {
            // 1. Получаем все завершенные и архивные путевые листы за период
            // (Считаем, что дата путевого листа = дата создания его первой записи или дата самого листа, 
            // но для точности лучше фильтровать по деталям поездок)
            var waybills = (await _waybillsRepo.GetWaybillsByDateRangeAsync(startDate, endDate))
                .Where(w => w.WaybillStatus == WaybillStatus.Completed || w.WaybillStatus == WaybillStatus.Archived);

            // 2. Фильтруем детали по датам (если нужно точное совпадение периода поездки)
            var relevantDetails = waybills
                .SelectMany(w => w.WaybillDetails)
                .Where(d => d.DepartureDateTime >= startDate && d.ArrivalDateTime <= endDate)
                .ToList();

            // 3. Считаем KPI
    

            // Суммарный пробег (сумма разниц спидометра по всем деталям)
            // Используем свойство Distance из сущности (EndMealing - StartMealing)
            float totalMileage = relevantDetails.Sum(d => d.Distance);

            // Суммарный расход топлива (ФАКТ)
            // Используем поле FuelConsumed, которое ввел менеджер
            float totalFuelConsumed = relevantDetails.Sum(d => d.FuelConsumed);

            return new DashboardKpi
            {
                TotalWaybills = waybills.Count(),
                TotalMileage = (float)Math.Round(totalMileage, 1),
                TotalFuelConsumed = (float)Math.Round(totalFuelConsumed, 1)
            };
        }

        /// <summary>
        /// 2. Получает отчет "План/Факт" по расходу топлива.
        /// </summary>
        public async Task<ICollection<FuelEfficiencyReportItem>> GetFuelEfficiencyReportAsync(DateTime startDate, DateTime endDate)
        {
            var waybills = (await _waybillsRepo.GetWaybillsByDateRangeAsync(startDate, endDate))
                .Where(w => w.WaybillStatus == WaybillStatus.Completed || 
                            w.WaybillStatus == WaybillStatus.Archived);

            var report = waybills
                .GroupBy(w => w.Car)
                .Select(group => 
                {
                    var car = group.Key;
                    var details = group.SelectMany(w => w.WaybillDetails)
                        .Where(d => d.DepartureDateTime >= startDate && d.ArrivalDateTime <= endDate);

                    // ФАКТ: Сумма того, что ввели в поле FuelConsumed
                    float totalFactFuel = details.Sum(d => d.FuelConsumed);

                    // НОРМА: Время * Расход
                    double totalNormFuel = details.Sum(d => d.NormalFuelConsumed);

                    return new FuelEfficiencyReportItem
                    {
                        CarModel = car.Model,
                        Distance = details.Sum(d => d.Distance),
                        FactFuel = (float)Math.Round(totalFactFuel, 2),
                        NormFuel = (float)Math.Round(totalNormFuel, 2),
                        Difference = (float)Math.Round(totalFactFuel - totalNormFuel, 2) // +Перерасход
                    };
                })
                .ToList();

            return report;
        }

        /// <summary>
        /// 3. Получает суммарный отчет по пробегу, сгруппированный по машинам.
        /// </summary>
        public async Task<ICollection<MileageReportItem>> GetMileageByCarAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAggregatedMileageReport(startDate, endDate, true);
        }

        /// <summary>
        /// 4. Получает суммарный отчет по пробегу, сгруппированный по водителям.
        /// </summary>
        public async Task<ICollection<MileageReportItem>> GetMileageByDriverAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAggregatedMileageReport(startDate, endDate, false);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Вспомогательный метод для получения путевых листов со всеми связанными данными.
        /// </summary>
        private async Task<List<Waybill>> GetCompletedWaybillsInRange(DateTime startDate, DateTime endDate)
        {
            // 1. Получаем все "шапки" Waybill за период
            var allWaybills = await _waybillsRepo.GetWaybillsByDateRangeAsync(startDate, endDate);
            
            // 2. Фильтруем "Завершенные"
            var completedWaybills = allWaybills
                .Where(w => w.WaybillStatus == WaybillStatus.Completed)
                .ToList();

            // 3. Получаем ВСЕ WaybillDetails и группируем их по WaybillId
            // Это намного эффективнее, чем N+1 запросов
            var allDetails = await _detailsRepo.GetAllAsync();
            var detailsLookup = allDetails.ToLookup(d => d.WaybillId);

            // 4. "Присоединяем" детали к их "шапкам"
            foreach (var wb in completedWaybills)
            {
                wb.WaybillDetails = detailsLookup[wb.Id].ToList();
            }

            return completedWaybills;
        }

        /// <summary>
        /// Общий вспомогательный метод для отчетов по пробегу.
        /// </summary>
        private async Task<ICollection<MileageReportItem>> GetAggregatedMileageReport(DateTime startDate, DateTime endDate, bool groupByCar)
        {
            var waybills = await GetCompletedWaybillsInRange(startDate, endDate);
            var cars = await _carsRepo.GetAllAsync();
            var drivers = await _driversRepo.GetAllAsync();

            var mileageData = new List<(string EntityName, float Mileage, int Trips)>();

            foreach (var wb in waybills)
            {
                var car = cars.FirstOrDefault(c => c.Id == wb.CarId);
                var driver = drivers.FirstOrDefault(d => d.Id == wb.DriverId);
                
                if (car == null || driver == null || !wb.WaybillDetails.Any())
                {
                    continue;
                }

                // Выбираем, по какому полю группировать
                string entityName = groupByCar ? car.Model : driver.DriverName;
                
                float totalMileage = wb.WaybillDetails.Sum(d => d.EndMealing - d.StartMealing);
                int totalTrips = wb.WaybillDetails.Count;
                
                mileageData.Add((entityName, totalMileage, totalTrips));
            }

            // Группируем и суммируем данные
            var groupedReport = mileageData
                .GroupBy(data => data.EntityName)
                .Select(g => new MileageReportItem
                {
                    EntityName = g.Key,
                    TotalMileage = (float)Math.Round(g.Sum(d => d.Mileage), 2),
                    TotalTrips = g.Sum(d => d.Trips)
                })
                .OrderByDescending(r => r.TotalMileage)
                .ToList();
            
            return groupedReport;
        }

        #endregion
    }
}