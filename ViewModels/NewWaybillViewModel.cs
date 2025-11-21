// В папке ViewModels/
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

namespace WaybillWpf.ViewModels
{
    public class NewWaybillViewModel : BaseViewModel
    {
        // --- Сервисы ---
        private readonly IWaybillFlowService _waybillFlowService;
        private readonly ICarManagementService _carService;
        private readonly IDriverManagementService _driverService;
        private readonly ICurrentUserService _currentUserService;

        // --- Списки для ComboBox ---
        private ObservableCollection<Car> _availableCars = new();
        public ObservableCollection<Car> AvailableCars
        {
            get => _availableCars;
            set
            {
                _availableCars = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Driver> _availableDrivers = new();
        public ObservableCollection<Driver> AvailableDrivers
        {
            get => _availableDrivers;
            set
            {
                _availableDrivers = value;
                OnPropertyChanged();
            }
        }

        // --- Выбранные элементы ---
        private Car? _selectedCar;
        public Car? SelectedCar
        {
            get => _selectedCar;
            set
            {
                _selectedCar = value;
                OnPropertyChanged(nameof(SelectedCar));
            }
        }

        private Driver? _selectedDriver;
        public Driver? SelectedDriver
        {
            get => _selectedDriver;
            set
            {
                _selectedDriver = value;
                OnPropertyChanged(nameof(SelectedDriver));
            }
        }

        // --- Состояние ---
        private bool _isLoading = true;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // --- Событие для View ---
        public event Action<bool>? RequestClose;

        // --- Команды ---
        public ICommand LoadDataCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public NewWaybillViewModel(IWaybillFlowService waybillFlowService, 
                                   ICarManagementService carService, 
                                   IDriverManagementService driverService, 
                                   ICurrentUserService currentUserService)
        {
            _waybillFlowService = waybillFlowService;
            _carService = carService;
            _driverService = driverService;
            _currentUserService = currentUserService;

            CreateCommand = new RelayCommand(async _ => await OnCreateAsync(), _ => CanCreate());
            CancelCommand = new RelayCommand(_ => OnCancel());
            LoadDataCommand = new RelayCommand(async _ => await OnLoadDataAsync());

            // Запускаем загрузку данных при создании VM
            LoadDataCommand.Execute(null);
        }

        private async Task OnLoadDataAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var cars = await _carService.GetAvailableCarsAsync();
                var drivers = await _driverService.GetAvailableDriversAsync();

                AvailableCars = new ObservableCollection<Car>(cars);
                AvailableDrivers = new ObservableCollection<Driver>(drivers);

                if (!AvailableCars.Any())
                    ErrorMessage = "Нет доступных машин.";
                else if (!AvailableDrivers.Any())
                    ErrorMessage = "Нет доступных водителей.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task OnCreateAsync()
        {
            ErrorMessage = string.Empty;
            if (!Validate()) return;

            try
            {
                int managerId = _currentUserService.CurrentUser!.Id;
                await _waybillFlowService.CreateDraftWaybillAsync(
                    managerId, 
                    SelectedCar!.Id, 
                    SelectedDriver!.Id);
                
                // Успех! Сообщаем View, что надо закрыться (с результатом true)
                RequestClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка создания: {ex.Message}";
            }
        }

        private void OnCancel()
        {
            // Сообщаем View, что надо закрыться (с результатом false)
            RequestClose?.Invoke(false);
        }
        
        private bool CanCreate()
        {
            return SelectedCar != null && SelectedDriver != null && !IsLoading;
        }

        private bool Validate()
        {
            if (SelectedCar == null)
            {
                ErrorMessage = "Выберите машину.";
                return false;
            }
            if (SelectedDriver == null)
            {
                ErrorMessage = "Выберите водителя.";
                return false;
            }
            return true;
        }
    }
}