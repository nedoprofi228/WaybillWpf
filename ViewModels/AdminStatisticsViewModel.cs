// ViewModels/Admin/AdminStatisticsViewModel.cs
// ... imports ...

using System.Collections.ObjectModel;
using System.Windows.Input;
using WaybillWpf.Domain.DTO;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

public class AdminStatisticsViewModel : BaseViewModel
{
    private readonly IStatisticService _statService;

    private DateTime _startDate;
    private DateTime _endDate;

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            _startDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
            OnPropertyChanged(); 
        }
    }
    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            _endDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
            OnPropertyChanged(); 
        }
    }

    // KPI
    private DashboardKpi? _dashboard;
    public DashboardKpi? Dashboard
    {
        get => _dashboard;
        set { _dashboard = value; OnPropertyChanged(); }
    }

    // Списки отчетов
    public ObservableCollection<FuelEfficiencyReportItem> FuelReport { get; set; } = new();
    public ObservableCollection<MileageReportItem> MileageByCarReport { get; set; } = new();
    public ObservableCollection<MileageReportItem> MileageByDriverReport { get; set; } = new();

    public ICommand LoadStatsCommand { get; }

    public AdminStatisticsViewModel(IStatisticService statService)
    {
        _statService = statService;
        LoadStatsCommand = new RelayCommand(async _ => await LoadAsync());
        
        // Автозагрузка при открытии
        LoadStatsCommand.Execute(null);
        StartDate = DateTime.Today.AddMonths(-1);
        EndDate =  DateTime.Today;
    }

    private async Task LoadAsync()
    {
        try
        {
            Dashboard = await _statService.GetDashboardKpisAsync(StartDate, EndDate);

            var fuel = await _statService.GetFuelEfficiencyReportAsync(StartDate, EndDate);
            FuelReport = new ObservableCollection<FuelEfficiencyReportItem>(fuel);
            OnPropertyChanged(nameof(FuelReport));

            var carStats = await _statService.GetMileageByCarAsync(StartDate, EndDate);
            MileageByCarReport = new ObservableCollection<MileageReportItem>(carStats);
            OnPropertyChanged(nameof(MileageByCarReport));

            var driverStats = await _statService.GetMileageByDriverAsync(StartDate, EndDate);
            MileageByDriverReport = new ObservableCollection<MileageReportItem>(driverStats);
            OnPropertyChanged(nameof(MileageByDriverReport));
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}");
        }
    }
}