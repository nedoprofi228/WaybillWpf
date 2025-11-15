
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
            // Получаем все нужные данные
            var waybills = await GetCompletedWaybillsInRange(startDate, endDate);
            var cars = await _carsRepo.GetAllAsync();
            
            var kpi = new DashboardKpi();

            if (!waybills.Any())
            {
                return kpi; // Возвращаем пустой KPI, если нет данных
            }

            kpi.TotalWaybillsCompleted = waybills.Count;
            kpi.TotalTripsCompleted = waybills.Sum(w => w.WaybillDetails.Count);

            foreach (var wb in waybills)
            {
                var car = cars.FirstOrDefault(c => c.Id == wb.CarId);
                if (car == null) continue;

                foreach (var detail in wb.WaybillDetails)
                {
                    // Используем исправленную логику из NotMapped свойств
                    float mileage = detail.EndMealing - detail.StartMealing;
                    float fuelSpent = detail.StartRemeaningFuel - detail.EndRemeaningFuel;

                    if (mileage > 0)
                    {
                        float fuelPlanned = (mileage / 100.0f) * car.FuelRate;
                        
                        kpi.TotalMileage += mileage;
                        kpi.TotalFuelSpent += fuelSpent;
                        kpi.TotalFuelDifference += (fuelSpent - fuelPlanned);
                    }
                }
            }
            return kpi;
        }

        /// <summary>
        /// 2. Получает отчет "План/Факт" по расходу топлива.
        /// </summary>
        public async Task<ICollection<FuelEfficiencyReportItem>> GetFuelEfficiencyReportAsync(DateTime startDate, DateTime endDate)
        {
            var waybills = await GetCompletedWaybillsInRange(startDate, endDate);
            var cars = await _carsRepo.GetAllAsync();
            var drivers = await _driversRepo.GetAllAsync();

            var reportItems = new List<FuelEfficiencyReportItem>();

            foreach (var wb in waybills)
            {
                var car = cars.FirstOrDefault(c => c.Id == wb.CarId);
                var driver = drivers.FirstOrDefault(d => d.Id == wb.DriverId);

                if (car == null || driver == null || !wb.WaybillDetails.Any())
                {
                    continue; // Пропускаем, если данные неполные
                }

                // Суммируем все отрезки (Details) для одного путевого листа (Waybill)
                float totalMileage = wb.WaybillDetails.Sum(d => d.EndMealing - d.StartMealing);
                float totalFuelSpent = wb.WaybillDetails.Sum(d => d.StartRemeaningFuel - d.EndRemeaningFuel);

                if (totalMileage <= 0)
                {
                    continue; // Пропускаем, если не было пробега
                }

                // Расчет плана
                float fuelPlanned = (totalMileage / 100.0f) * car.FuelRate;
                
                reportItems.Add(new FuelEfficiencyReportItem
                {
                    CarModel = car.Model,
                    DriverName = driver.DriverName,
                    TotalMileage = totalMileage,
                    FuelRateNorm = car.FuelRate,
                    FuelPlanned = (float)Math.Round(fuelPlanned, 2),
                    FuelActual = (float)Math.Round(totalFuelSpent, 2),
                    Difference = (float)Math.Round(totalFuelSpent - fuelPlanned, 2) // > 0 = перерасход
                });
            }

            // Группируем результат, чтобы получить итог по каждой машине/водителю
            var groupedReport = reportItems
                .GroupBy(item => new { item.CarModel, item.DriverName })
                .Select(g => new FuelEfficiencyReportItem
                {
                    CarModel = g.Key.CarModel,
                    DriverName = g.Key.DriverName,
                    FuelRateNorm = g.First().FuelRateNorm, // Норматив у них один
                    TotalMileage = (float)Math.Round(g.Sum(i => i.TotalMileage), 2),
                    FuelPlanned = (float)Math.Round(g.Sum(i => i.FuelPlanned), 2),
                    FuelActual = (float)Math.Round(g.Sum(i => i.FuelActual), 2),
                    Difference = (float)Math.Round(g.Sum(i => i.Difference), 2)
                })
                .OrderByDescending(r => r.Difference) // Сверху худшие (наибольший перерасход)
                .ToList();
                
            return groupedReport;
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