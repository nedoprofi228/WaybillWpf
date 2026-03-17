using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;
using WaybillWpf.Views;

namespace WaybillWpf.ViewModels;

public class DriverManagementViewModel : BaseViewModel
{
    private readonly IWaybillDetailsRepository _waybillDetailsRepository;
    private readonly IWaybillsRepository _waybillsRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IWaybillStateTransitionsService _WaybillStateTransitionsService;

    private Waybill? _selectedWaybill;

    public Waybill? SelectedWaybill
    {
        get => _selectedWaybill;
        set
        {
            _selectedWaybill = value;
            OnPropertyChanged(nameof(SelectedWaybill));
            OnPropertyChanged(nameof(IsDetailPanelVisible));
            OnPropertyChanged(nameof(CanEditDetails));
            OnPropertyChanged(nameof(CanAccepting));
            OnPropertyChanged(nameof(CanEditWaybil));
        }
    }

    private WaybillDetails? _selectedWaybillDetails;

    public WaybillDetails? SelectedWaybillDetails
    {
        get => _selectedWaybillDetails;
        set
        {
            _selectedWaybillDetails = value;
            OnPropertyChanged(nameof(SelectedWaybillDetails));
            OnPropertyChanged(nameof(CanEditDetails));
        }
    }

    private ObservableCollection<Waybill> _waybillsList;

    public ObservableCollection<Waybill> WaybillsList
    {
        get => _waybillsList;
        set
        {
            _waybillsList = value;
            OnPropertyChanged(nameof(WaybillsList));
        }
    }

    public bool CanEditWaybil => SelectedWaybill != null && (SelectedWaybill?.WaybillStatus == WaybillStatus.InProgress || SelectedWaybill?.WaybillStatus == WaybillStatus.Declinede);

    public bool CanEditDetails => CanEditWaybil && SelectedWaybill!.WaybillDetails.Count > 0 ? SelectedWaybillDetails != null : true;
    
    public bool CanAccepting => CanEditWaybil && (bool)SelectedWaybill?.WaybillDetails.Any();
    public bool IsDetailPanelVisible => SelectedWaybill != null;

    public ICommand AddNewDetailCommand { get; }
    public ICommand EditDetailCommand { get; }
    public ICommand DeleteWaybillDetailCommand { get; }
    public ICommand AcceptingWaybillCommand { get; }
    public ICommand LogOutCommand { get; }

    public event Action? CloseAction;

    public DriverManagementViewModel(
        IWaybillStateTransitionsService waybillStateTransitionsService,
        IWaybillDetailsRepository waybillDetailsRepository,
        ICurrentUserService currentUserService,
        IWaybillsRepository waybillsRepository)
    {
        _waybillDetailsRepository = waybillDetailsRepository;
        _waybillsRepository = waybillsRepository;
        _currentUserService = currentUserService;
        _WaybillStateTransitionsService = waybillStateTransitionsService;

        DeleteWaybillDetailCommand = new RelayCommand(async void (_) => await OnDeleteWaybillDetailAsync(), _ => CanEditDetails);
        AddNewDetailCommand = new RelayCommand(async void (_) => await OnAddNewDetailAsync(), _ => CanEditWaybil);
        EditDetailCommand = new RelayCommand(async void (_) => await EditDetailAsync(), _ => CanEditDetails);
        AcceptingWaybillCommand = new RelayCommand(async void (_) => await AcceptingWaybillAsync(), _ => CanAccepting);

        LogOutCommand = new RelayCommand(_ =>
        {
            ServicesProvider.GetService<CurrentUserService>()?.Logout();
            ServicesProvider.GetService<LoginView>()?.Show();
            CloseAction?.Invoke();
        });

        WaybillsList = new ObservableCollection<Waybill>();
        LoadDataAsync();
    }

    private async Task OnAddNewDetailAsync()
    {
        var vmDetails = ServicesProvider.GetService<WaybillDetailEditorViewModel>();
        var windowDetails = ServicesProvider.GetService<WaybillDetailEditorView>();

        if (windowDetails == null || vmDetails == null)
        {
            MessageBox.Show("Ошибка: нет сервиса или окна");
            return;
        }

        // В Initialize теперь передаем 0,0 и расход машины (если обновили метод)
        // Либо ваш старый вариант:
        float carRate = SelectedWaybill?.Car?.FuelRate ?? 0;
        decimal fuelPrice = SelectedWaybill?.Car?.FuelType?.Price ?? 0;
        vmDetails.Initialize(SelectedWaybill!, carRate, fuelPrice);

        vmDetails.RequestClose += (_) => windowDetails.Close();

        windowDetails.DataContext = vmDetails;
        windowDetails.ShowDialog();

        if (vmDetails.ResultDetail == null)
        {
            return;
        }

        try
        {
            await _waybillDetailsRepository.AddAsync(vmDetails.ResultDetail);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка добавления детали: {ex.Message}");
        }
    }

    private async Task EditDetailAsync()
    {
        if (SelectedWaybillDetails == null) return;

        var vmDetails = ServicesProvider.GetService<WaybillDetailEditorViewModel>();
        var windowDetails = ServicesProvider.GetService<WaybillDetailEditorView>();

        if (windowDetails == null || vmDetails == null)
        {
            MessageBox.Show("Ошибка: нет сервиса или окна");
            return;
        }

        float carRate = SelectedWaybill?.Car?.FuelRate ?? 0;
        vmDetails.Initialize(SelectedWaybill, SelectedWaybillDetails);

        vmDetails.RequestClose += (_) => windowDetails.Close();

        windowDetails.DataContext = vmDetails;
        windowDetails.ShowDialog();

        if (vmDetails.ResultDetail == null) return;

        try
        {
            await _waybillDetailsRepository.UpdateAsync(vmDetails.ResultDetail);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка изменения детали: {ex.Message}");
        }
    }

    private async Task OnDeleteWaybillDetailAsync()
    {
        if (SelectedWaybillDetails == null || SelectedWaybill == null)
            return;

        var result = MessageBox.Show(
            "Вы уверены, что хотите удалить этот отрезок пути?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _waybillDetailsRepository.DeleteAsync(SelectedWaybillDetails);
            SelectedWaybill.WaybillDetails.Remove(SelectedWaybillDetails);
            await LoadDataAsync(); // Лучше перезагрузить
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении отрезка пути: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public async Task AcceptingWaybillAsync()
    {
        if (SelectedWaybill == null) return;

        var result = MessageBox.Show(
            "Вы уверены, что хотите отправить на завершение путевой лист, позже внести изменения не получиться?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        if (!await _WaybillStateTransitionsService.AcceptingWaybillAsync(SelectedWaybill.Id))
        {
            MessageBox.Show("Не получилось отправить на завершение");
            return;
        }

        await LoadDataAsync();
        OnPropertyChanged(nameof(CanEditWaybil));
    }

    private async Task LoadDataAsync()
    {
        if (_currentUserService.CurrentUser == null) return;

        WaybillsList.Clear(); // Тут можно оставить Clear, но лучше создавать новую коллекцию
        int managerId = _currentUserService.CurrentUser.Id;

        var waybills = await _waybillsRepository.GetWaybillsByDriverIdAsync(managerId);
        WaybillsList = new ObservableCollection<Waybill>(waybills.Where(w => w.WaybillStatus != WaybillStatus.Draft).OrderByDescending(w => w.AtCreated));
    }
}