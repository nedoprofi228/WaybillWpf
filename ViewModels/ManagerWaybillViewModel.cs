// В папке ViewModels/
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; // Для MessageBox
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Exceptions;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;
using WaybillWpf.Views;

namespace WaybillWpf.ViewModels
{
    public class ManagerWaybillViewModel : BaseViewModel
    {
        // --- Сервисы ---
        private readonly IWaybillFlowService _waybillFlowService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDriverManagementService _driverManagementService;
        private readonly ICarManagementService _carManagementService;
        
        private ICollection<Driver> _availableDrivers = new List<Driver>();
        private ICollection<Car> _availableCars = new List<Car>();

        // --- Списки ---
        private ObservableCollection<Waybill> _waybillsList = new();

        public ObservableCollection<Waybill> WaybillsList
        {
            get => _waybillsList;
            set
            {
                _waybillsList = value;
                OnPropertyChanged("WaybillsList");
            }
        }

        // --- Выбранный элемент ---
        private Waybill? _selectedWaybill;

        public Waybill? SelectedWaybill
        {
            get => _selectedWaybill;
            set
            {
                _selectedWaybill = value;
                OnPropertyChanged("SelectedWaybill");
                UpdateDetailPanelState();
            }
        }

        private WaybillDetails? _selectedWaybillDetails;

        public WaybillDetails? SelectedWaybillDetails
        {
            get => _selectedWaybillDetails;
            set
            {
                _selectedWaybillDetails = value;
                OnPropertyChanged("SelectedWaybillDetails");

                if (value != null && CanEditDetails)
                    OnSelectedWaybillDetails();
            }
        }

        // === НОВОЕ СВОЙСТВО: Выбранное задание ===
        private WaybillTask? _selectedTask;
        public WaybillTask? SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged(nameof(SelectedTask));

                if (value != null && CanEditDetails)
                    OnSelectedTask(); // Открываем редактор при клике
            }
        }

        // --- Состояние UI ---
        public bool IsDetailPanelVisible => SelectedWaybill != null;
        public bool CanEditDetails => SelectedWaybill?.WaybillStatus == WaybillStatus.InProgress; 
        public bool CanIssue => SelectedWaybill?.WaybillStatus == WaybillStatus.Draft;
        public bool CanComplete => SelectedWaybill?.WaybillStatus == WaybillStatus.InProgress;
        public bool CanArchive => SelectedWaybill?.WaybillStatus == WaybillStatus.Completed;
        public bool CanDeleteWaybill => SelectedWaybill?.WaybillStatus == WaybillStatus.Draft;

        // --- Команды ---
        public ICommand LoadCommand { get; }
        public ICommand AddWaybillCommand { get; }
        public ICommand ArchiveWaybillCommand { get; }
        public ICommand AddNewDetailCommand { get; }
        public ICommand IssueWaybillCommand { get; }
        public ICommand CompleteWaybillCommand { get; }
        public ICommand DeleteWaybillDetailCommand { get; }
        public ICommand DeleteWaybillCommand { get; } // Команда удаления черновика (если нужна)

        // === НОВЫЕ КОМАНДЫ ===
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        // --- Конструктор ---
        public ManagerWaybillViewModel(IWaybillFlowService waybillFlowService, ICurrentUserService currentUserService, IDriverManagementService driverManagementService, ICarManagementService carManagementService)
        {
            _waybillFlowService = waybillFlowService;
            _currentUserService = currentUserService;
            _driverManagementService = driverManagementService;
            _carManagementService = carManagementService;

            // Инициализация команд
            LoadCommand = new RelayCommand(async void (_) => await LoadDataAsync());
            AddWaybillCommand = new RelayCommand(async void (_) => await OnAddWaybillAsync());
            ArchiveWaybillCommand = new RelayCommand(async void (_) => await OnArchiveWaybillAsync(), _ => CanArchive);
            AddNewDetailCommand = new RelayCommand(async void (_) => await OnAddNewDetailAsync(), _ => CanEditDetails);
            IssueWaybillCommand = new RelayCommand(async void (_) => await OnIssueWaybillAsync(), _ => CanIssue);
            CompleteWaybillCommand = new RelayCommand(async void (_) => await OnCompleteWaybillAsync(), _ => CanComplete);
            DeleteWaybillDetailCommand = new RelayCommand(async void (_) => await OnDeleteWaybillDetailAsync(), _ => CanEditDetails);
            DeleteWaybillCommand = new RelayCommand(async void (p) => await OnDeleteWaybillAsync(p)); // Если добавляли

            // === Инициализация НОВЫХ команд ===
            AddTaskCommand = new RelayCommand(async void (_) => await OnAddTaskAsync(), _ => CanEditDetails);
            DeleteTaskCommand = new RelayCommand(async void (param) => await OnDeleteTaskAsync(param!), _ => CanEditDetails);

            //Загружаем данные при старте
            LoadCommand.Execute(null);
        }

        private async Task LoadDataAsync()
        {
            if (_currentUserService.CurrentUser == null) return;

            WaybillsList.Clear(); // Тут можно оставить Clear, но лучше создавать новую коллекцию
            int managerId = _currentUserService.CurrentUser.Id;

            var waybills = await _waybillFlowService.GetWaybillsAsync(managerId, null);
            WaybillsList = new ObservableCollection<Waybill>(waybills.OrderByDescending(w => w.Id));

            _availableCars = await _carManagementService.GetAvailableCarsAsync();
            _availableDrivers = await _driverManagementService.GetAvailableDriversAsync();
        }

        private async Task OnAddWaybillAsync()
        {
            var newWaybillDialog = ServicesProvider.GetService<NewWaybillView>();
            if (newWaybillDialog == null)
            {
                MessageBox.Show("Ошибка: Не удалось открыть окно создания.");
                return;
            }

            bool? result = newWaybillDialog.ShowDialog();

            if (result == true)
            {
                await LoadDataAsync();
            }
        }

        // Метод удаления черновика (добавил, чтобы работала кнопка в таблице)
        private async Task OnDeleteWaybillAsync(object? parameter)
        {
            if (SelectedWaybill == null) return;
            if (MessageBox.Show("Удалить черновик?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            try 
            { 
                await _waybillFlowService.DeleteWaybillAsync(SelectedWaybill.Id); 
                await LoadDataAsync();
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
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
            vmDetails.Initialize(SelectedWaybill!, carRate); 

            vmDetails.RequestClose += (_) => windowDetails.Close();

            windowDetails.DataContext = vmDetails;
            windowDetails.ShowDialog();

            if (vmDetails.ResultDetail == null)
            {
                return;
            }

            try
            {
                await _waybillFlowService.AddDetailAsync(vmDetails.ResultDetail);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления детали: {ex.Message}");
            }
        }

        private bool CheckDriverAndCar()
        {
            if (SelectedWaybill == null)
                return false;

            if (!_availableCars.Contains(SelectedWaybill.Car) || !_availableDrivers.Contains(SelectedWaybill.Driver))
            {
                MessageBox.Show("Выбранная машина или водитель сейчас находятся в другом рейсе");
                return false;
            }

            return true;
        }

        private async void OnSelectedWaybillDetails()
        {
            var vmDetails = ServicesProvider.GetService<WaybillDetailEditorViewModel>();
            var windowDetails = ServicesProvider.GetService<WaybillDetailEditorView>();
            if (windowDetails == null || vmDetails == null)
            {
                MessageBox.Show("Ошибка: нет сервиса или окна");
                return;
            }

            // Инициализация
            float carRate = SelectedWaybill?.Car?.FuelRate ?? 0;
            vmDetails.Initialize(SelectedWaybill!, SelectedWaybillDetails!);
            
            // Заполняем данными из выбранной детали
            vmDetails.DepartureDateTime = SelectedWaybillDetails!.DepartureDateTime;
            vmDetails.ArrivalDateTime = SelectedWaybillDetails.ArrivalDateTime;
            vmDetails.StartMealing = SelectedWaybillDetails.StartMealing;
            vmDetails.EndMealing = SelectedWaybillDetails.EndMealing;
            vmDetails.StartFuel = SelectedWaybillDetails.StartRemeaningFuel;
            vmDetails.EndFuel = SelectedWaybillDetails.EndRemeaningFuel;
            vmDetails.FuelConsumedInput = SelectedWaybillDetails.FuelConsumed;


            vmDetails.RequestClose += (_) => windowDetails.Close();

            windowDetails.DataContext = vmDetails;
            windowDetails.ShowDialog();

            if (vmDetails.ResultDetail != null)
            {
                SelectedWaybillDetails!.StartMealing = vmDetails.ResultDetail.StartMealing;
                SelectedWaybillDetails.EndMealing = vmDetails.ResultDetail.EndMealing;
                SelectedWaybillDetails.FuelConsumed = vmDetails.ResultDetail.FuelConsumed;
                SelectedWaybillDetails.ArrivalDateTime = vmDetails.ResultDetail.ArrivalDateTime;
                SelectedWaybillDetails.DepartureDateTime = vmDetails.ResultDetail.DepartureDateTime;
                SelectedWaybillDetails.StartRemeaningFuel = vmDetails.ResultDetail.StartRemeaningFuel;
                SelectedWaybillDetails.EndRemeaningFuel = vmDetails.ResultDetail.EndRemeaningFuel;

                await _waybillFlowService.UpdateDetailAsync(SelectedWaybillDetails);
                await LoadDataAsync();
            }

            SelectedWaybillDetails = null;
        }

        // === НОВЫЕ МЕТОДЫ ДЛЯ ЗАДАНИЙ (СКОПИРОВАНО С ДЕТАЛЕЙ) ===

        private async Task OnAddTaskAsync()
        {
            var vmTask = ServicesProvider.GetService<WaybillTaskEditorViewModel>();
            var windowTask = ServicesProvider.GetService<WaybillTaskEditorView>();

            if (windowTask == null || vmTask == null)
            {
                MessageBox.Show("Ошибка: нет сервиса или окна задач");
                return;
            }

            // Инициализация для нового задания
            vmTask.Initialize(SelectedWaybill!.Id);
            vmTask.RequestClose += (_) => windowTask.Close();

            windowTask.DataContext = vmTask;
            windowTask.ShowDialog();

            if (vmTask.ResultTask == null) return;

            try
            {
                await _waybillFlowService.AddTaskAsync(vmTask.ResultTask);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления задания: {ex.Message}");
            }
        }

        private async void OnSelectedTask()
        {
            var vmTask = ServicesProvider.GetService<WaybillTaskEditorViewModel>();
            var windowTask = ServicesProvider.GetService<WaybillTaskEditorView>();

            if (windowTask == null || vmTask == null) return;

            // Инициализация для редактирования (передаем существующую задачу)
            vmTask.Initialize(SelectedWaybill!.Id, SelectedTask);
            vmTask.RequestClose += (_) => windowTask.Close();

            windowTask.DataContext = vmTask;
            windowTask.ShowDialog();

            if (vmTask.ResultTask != null)
            {
                // Обновляем текущий объект
                SelectedTask!.Date = vmTask.ResultTask.Date;
                SelectedTask.DeparturePoint = vmTask.ResultTask.DeparturePoint;
                SelectedTask.ArrivalPoint = vmTask.ResultTask.ArrivalPoint;
                SelectedTask.Mileage = vmTask.ResultTask.Mileage;
                SelectedTask.CustomerName = vmTask.ResultTask.CustomerName;
                SelectedTask.OtherInfo = vmTask.ResultTask.OtherInfo;

                await _waybillFlowService.UpdateTaskAsync(SelectedTask);
                await LoadDataAsync();
            }
            SelectedTask = null;
        }

        private async Task OnDeleteTaskAsync(object parameter)
        {
            if (!(parameter is WaybillTask taskToDelete) || SelectedWaybill == null)
                return;

            if (MessageBox.Show("Удалить маршрут?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                await _waybillFlowService.DeleteTaskAsync(taskToDelete.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // === КОНЕЦ НОВЫХ МЕТОДОВ ===

        private async Task OnIssueWaybillAsync()
        {
            if (!CheckDriverAndCar()) return;
            
            var result = MessageBox.Show(
                "Вы уверены, что хотите 'Выдать' этот путевой лист?\n\n",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
            
            if(!_availableCars.Contains(SelectedWaybill.Car!) &&
               !_availableDrivers.Contains(SelectedWaybill.Driver!))
            {
                MessageBox.Show("Вы не можете выдать его т.к. водитель или машина уже заняты");
                // (тут можно return, но иногда хотят продолжить)
            }

            try
            {
                await _waybillFlowService.IssueWaybillAsync(SelectedWaybill!.Id);
                await LoadDataAsync();
            }
            catch (WaybillValidationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task OnCompleteWaybillAsync()
        {
            if (SelectedWaybill == null) return;

            var result = MessageBox.Show(
                "Вы уверены, что хотите завершить этот рейс?\n" +
                "Убедитесь, что все показания счетчиков и топлива внесены корректно.",
                "Завершение рейса",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _waybillFlowService.CompleteWaybillAsync(SelectedWaybill.Id);
                MessageBox.Show("Рейс успешно завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private async Task OnArchiveWaybillAsync()
        {
            if (SelectedWaybill == null) return;

            var result = MessageBox.Show(
                "Отправить путевой лист в архив?\n" +
                "После этого он станет недоступен для редактирования и текущего просмотра.",
                "Архивация",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _waybillFlowService.ArchiveWaybillAsync(SelectedWaybill.Id);
                WaybillsList.Remove(SelectedWaybill);
                SelectedWaybill = null;

                MessageBox.Show("Путевой лист отправлен в архив.", "Успех", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка архивации: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
                await _waybillFlowService.DeleteDetailAsync(SelectedWaybillDetails.Id);
                SelectedWaybill.WaybillDetails.Remove(SelectedWaybillDetails);
                // SelectedWaybill = null; // Не обязательно сбрасывать
                await LoadDataAsync(); // Лучше перезагрузить
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении отрезка пути: {ex.Message}", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateDetailPanelState()
        {
            OnPropertyChanged(nameof(IsDetailPanelVisible));
            OnPropertyChanged(nameof(CanEditDetails));
            OnPropertyChanged(nameof(CanIssue));
            OnPropertyChanged(nameof(CanComplete));
            OnPropertyChanged(nameof(CanArchive));
            OnPropertyChanged(nameof(CanDeleteWaybill));
        }
        
    }
}