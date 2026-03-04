using System;
using System.Collections.ObjectModel;
using System.Linq; // Важно для фильтрации
using System.Threading.Tasks;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using WaybillWpf.Domain.DTO;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

public class AdminStatisticsViewModel : BaseViewModel
{
    private readonly IStatisticService _statService;

    // === ФИЛЬТРЫ ===
    private DateTime _startDate;
    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            _startDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
            OnPropertyChanged(); 
        }
    }

    private DateTime _endDate;
    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            _endDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
            OnPropertyChanged(); 
        }
    }

    // НОВОЕ: Поле для поиска
    private string _searchCarModel;
    public string SearchCarModel
    {
        get => _searchCarModel;
        set
        {
            _searchCarModel = value;
            OnPropertyChanged();
        }
    }

    // === KPI ===
    private DashboardKpi? _dashboard;
    public DashboardKpi? Dashboard
    {
        get => _dashboard;
        set { _dashboard = value; OnPropertyChanged(); }
    }
    
    // === ГРАФИК ===
    private SeriesCollection _fuelChartSeries;
    public SeriesCollection FuelChartSeries
    {
        get => _fuelChartSeries;
        set { _fuelChartSeries = value; OnPropertyChanged(); }
    }

    private string[] _chartLabels;
    public string[] ChartLabels
    {
        get => _chartLabels;
        set { _chartLabels = value; OnPropertyChanged(); }
    }

    public Func<double, string> ChartFormatter { get; set; }

    // === СПИСКИ ===
    public ObservableCollection<FuelEfficiencyReportItem> FuelReport { get; set; } = new();
    public ObservableCollection<MileageReportItem> MileageByCarReport { get; set; } = new();
    public ObservableCollection<MileageReportItem> MileageByDriverReport { get; set; } = new();

    public ICommand LoadStatsCommand { get; }

    public AdminStatisticsViewModel(IStatisticService statService)
    {
        _statService = statService;
        LoadStatsCommand = new RelayCommand(async _ => await LoadAsync());
        
        ChartFormatter = value => value.ToString("N0");
        
        // Значения по умолчанию
        StartDate = DateTime.Today.AddMonths(-1);
        EndDate = DateTime.Today;

        // Автозагрузка
        LoadStatsCommand.Execute(null);
    }

    private async Task LoadAsync()
    {
        try
        {
            // 1. Загружаем "сырые" данные из сервиса
            Dashboard = await _statService.GetDashboardKpisAsync(StartDate, EndDate);
            var fuelData = await _statService.GetFuelEfficiencyReportAsync(StartDate, EndDate);
            var carStats = await _statService.GetMileageByCarAsync(StartDate, EndDate);
            var driverStats = await _statService.GetMileageByDriverAsync(StartDate, EndDate); // Водителей обычно не фильтруют по модели машины, оставляем как есть или доработаем при необходимости

            // 2. === ФИЛЬТРАЦИЯ ПО МОДЕЛИ ===
            if (!string.IsNullOrWhiteSpace(SearchCarModel))
            {
                var filter = SearchCarModel.ToLower().Trim();

                // Фильтруем отчет по топливу (там есть CarModel)
                fuelData = fuelData
                    .Where(x => x.CarModel != null && x.CarModel.ToLower().Contains(filter))
                    .ToList();

                // Фильтруем отчет по пробегу машин (предполагаем, что EntityName это модель или содержит её)
                carStats = carStats
                    .Where(x => x.EntityName != null && x.EntityName.ToLower().Contains(filter))
                    .ToList();
            }

            // 3. Заполняем ObservableCollections отфильтрованными данными
            FuelReport = new ObservableCollection<FuelEfficiencyReportItem>(fuelData);
            MileageByCarReport = new ObservableCollection<MileageReportItem>(carStats);
            MileageByDriverReport = new ObservableCollection<MileageReportItem>(driverStats);
            
            OnPropertyChanged(nameof(FuelReport));
            OnPropertyChanged(nameof(MileageByCarReport));
            OnPropertyChanged(nameof(MileageByDriverReport));

            // 4. === СТРОИМ ГРАФИК (по отфильтрованным данным) ===
            
            // Если данных нет после фильтра - очищаем график
            if (!fuelData.Any())
            {
                ChartLabels = new string[0];
                FuelChartSeries = new SeriesCollection();
                return;
            }

            // Ось X - Названия машин
            ChartLabels = fuelData.Select(x => x.CarModel).ToArray(); 

            // Столбцы
            FuelChartSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Факт (л)",
                    Values = new ChartValues<float>(fuelData.Select(x => x.FactFuel)),
                    Fill = System.Windows.Media.Brushes.IndianRed
                },
                new ColumnSeries
                {
                    Title = "Норма (л)",
                    Values = new ChartValues<float>(fuelData.Select(x => x.NormFuel)),
                    Fill = System.Windows.Media.Brushes.CornflowerBlue
                }
            };
        }
        catch (Exception ex)
        {
            // В реальном проекте лучше использовать сервис диалогов или логирование
            System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}