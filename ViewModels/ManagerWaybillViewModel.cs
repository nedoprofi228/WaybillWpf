//В папке ViewModels/
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
        // (Могут понадобиться ICar/IDriver сервисы для диалога "Добавить")

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
                
                if(value != null)
                    OnSelectedWaybillDetails();
            }
        }

        // --- Состояние UI ---
public bool IsDetailPanelVisible => SelectedWaybill != null;
        public bool CanEditDetails => SelectedWaybill?.WaybillStatus == WaybillStatus.Draft;
        public bool CanIssue => SelectedWaybill?.WaybillStatus == WaybillStatus.Draft;
        public bool CanComplete => SelectedWaybill?.WaybillStatus == WaybillStatus.InProgress;
        public bool CanArchive => SelectedWaybill?.WaybillStatus == WaybillStatus.Completed;

        // --- Команды ---
        public ICommand LoadCommand { get; }
        public ICommand AddWaybillCommand { get; }
        public ICommand ArchiveWaybillCommand { get;}
public ICommand AddNewDetailCommand { get; }
        public ICommand IssueWaybillCommand { get; }
        public ICommand CompleteWaybillCommand { get; }
public ICommand DeleteWaybillDetailCommand { get; }

       // --- Конструктор ---
        public ManagerWaybillViewModel(IWaybillFlowService waybillFlowService, ICurrentUserService currentUserService)
        {
            _waybillFlowService = waybillFlowService;
            _currentUserService = currentUserService;

            // Инициализация команд
            LoadCommand = new RelayCommand(async _=> await LoadWaybillsAsync());
            AddWaybillCommand = new RelayCommand(async _ =>await OnAddWaybillAsync());
            ArchiveWaybillCommand = new RelayCommand(async _ => await OnArchiveWaybillAsync(), _ => CanArchive);
            AddNewDetailCommand = new RelayCommand(async _ => await OnAddNewDetailAsync(), _ => CanEditDetails);
            IssueWaybillCommand = new RelayCommand(async _ =>await OnIssueWaybillAsync(), _ => CanIssue);
            CompleteWaybillCommand = new RelayCommand(async _ => await OnCompleteWaybillAsync(), _ => CanComplete);
           DeleteWaybillDetailCommand = new RelayCommand(async param => await OnDeleteWaybillDetailAsync(param), _ => CanEditDetails);
            
            //Загружаем данные при старте
            LoadCommand.Execute(null);
        }

private async Task LoadWaybillsAsync()
        {
            if (_currentUserService.CurrentUser == null) return;
            
            WaybillsList.Clear();
            int managerId = _currentUserService.CurrentUser.Id;

            // Загружаем только листы этогоменеджера
            var waybills = await _waybillFlowService.GetWaybillsAsync(managerId, null);
            WaybillsList = new ObservableCollection<Waybill>(waybills.OrderByDescending(w => w.Id));
        }

        private async Task OnAddWaybillAsync()
        {
            // 1. Получаем новое окноиз DI
            var newWaybillDialog = ServicesProvider.GetService<NewWaybillView>();
            if (newWaybillDialog == null)
            {
                MessageBox.Show("Ошибка: Не удалось открыть окно создания.");
                return;
}

            // 2. Открываем окно МОДАЛЬНО (ShowDialog)
            // Код здесь остановится, пока пользователь не нажмет "Создать" или "Отмена"
            bool? result = newWaybillDialog.ShowDialog();

            // 3.Проверяем результат
            if (result == true)
            {
                //Пользователь нажал "Создать" и все прошло успешно.
                // Просто перезагружаем список, чтобы увидеть новый черновик.
                await LoadWaybillsAsync();
            }
           // Если result == false или null,пользователь нажал "Отмена"
            // и ничего делать не нужно.
        }

        private async Task OnAddNewDetailAsync()
        {
            var vmDetails = ServicesProvider.GetService<WaybillDetailEditorViewModel>();
            var windowDetails = ServicesProvider.GetService<WaybillDetailEditorView>();

            if (windowDetails == null || vmDetails== null)
            {
                MessageBox.Show("Ошибка: нет сервиса или окна");
                return;
            }
            vmDetails.Initialize(SelectedWaybill, SelectedWaybill.Car!.FuelRate);
            vmDetails.RequestClose += (_) => windowDetails.Close();
            
            windowDetails.DataContext =vmDetails;
            windowDetails.ShowDialog();

            if (vmDetails.ResultDetail == null)
            {
                return;
            }

            try
            {
                await _waybillFlowService.AddDetailAsync(vmDetails.ResultDetail);
                // Обновляем детали в UI
               await LoadWaybillsAsync();
            }
catch (Exception ex)
            {
MessageBox.Show($"Ошибка добавления детали: {ex.Message}");
            }
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
            
            vmDetails.Initialize(SelectedWaybill, SelectedWaybillDetails);
            vmDetails.RequestClose += (_) => windowDetails.Close();
            
            windowDetails.DataContext = vmDetails;
            windowDetails.ShowDialog();
            
            if (vmDetails.ResultDetail != null)
            {
                SelectedWaybillDetails.StartMealing = vmDetails.ResultDetail.StartMealing;
                SelectedWaybillDetails.EndMealing = vmDetails.ResultDetail.EndMealing;
                SelectedWaybillDetails.FuelConsumed = vmDetails.ResultDetail.FuelConsumed;
                SelectedWaybillDetails.ArrivalDateTime = vmDetails.ResultDetail.ArrivalDateTime;
                SelectedWaybillDetails.DepartureDateTime = vmDetails.ResultDetail.DepartureDateTime;
                SelectedWaybillDetails.StartRemeaningFuel = vmDetails.ResultDetail.StartRemeaningFuel;
                SelectedWaybillDetails.EndRemeaningFuel = vmDetails.ResultDetail.EndRemeaningFuel;
                
                await _waybillFlowService.UpdateDetailAsync(SelectedWaybillDetails);
                await LoadWaybillsAsync();
            }

            SelectedWaybillDetails = null;
        }
        
       private async Task OnIssueWaybillAsync()
        {
            // --- ТО САМОЕ ПОДТВЕРЖДЕНИЕ ---
            var result = MessageBox.Show(
                "Вы уверены, что хотите 'Выдать' этот путевой лист?\n\n" +
                "После этого вы НЕ СМОЖЕТЕ изменять детали или добавлять новые отрезки.",
                "Подтверждение", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _waybillFlowService.IssueWaybillAsync(SelectedWaybill!.Id);
                // Обновляем состояние
                await LoadWaybillsAsync(); 
            }
            catch (WaybillValidationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task OnCompleteWaybillAsync()
        {
            if (SelectedWaybill == null) return;

            // 1. Спрашиваем подтверждение
           var result = MessageBox.Show(
"Вы уверены, что хотите завершить этот рейс?\n" +
                "Убедитесь, что все показания счетчиков и топлива внесены корректно.",
                "Завершение рейса",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try{
                // 2. Вызываем сервис
                await _waybillFlowService.CompleteWaybillAsync(SelectedWaybill.Id);
                
                // 3. Обновляем UI
                MessageBox.Show("Рейс успешно завершен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
await LoadWaybillsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task OnArchiveWaybillAsync()
        {
            if (SelectedWaybill ==null) return;

            // 1. Спрашиваем подтверждение
            var result = MessageBox.Show(
                "Отправить путевой лист в архив?\n" +
                "После этого он станет недоступен для редактирования и текущего просмотра.",
                "Архивация",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // 2. Вызываем сервис
                await _waybillFlowService.ArchiveWaybillAsync(SelectedWaybill.Id);

               // 3. Обновляем список
                // Тут есть два варианта:
                // Вариант А: Просто обновить статус (он станет серым/неактивным)
                // await RefreshSelectedWaybillDetails();
                
                // Вариант Б: Убрать его из списка (если менеджер видит только активные)
                // Удаляем из ObservableCollection вручную, чтобы не дергать БД лишний раз
                WaybillsList.Remove(SelectedWaybill);
                SelectedWaybill =null; // Сбрасываем выделение
                
                MessageBox.Show("Путевой лист отправлен в архив.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка архивации: {ex.Message}","Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnDeleteWaybillDetailAsync(object parameter)
        {
            if (!(parameter is WaybillDetails detailToDelete) || SelectedWaybill == null)
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
                await _waybillFlowService.DeleteDetailAsync(detailToDelete.Id);
                SelectedWaybill.WaybillDetails.Remove(detailToDelete);
                SelectedWaybill = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении отрезка пути: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Вспомогательные методы ---

        private void UpdateDetailPanelState()
        {
            // Уведомляем UI о том, что состояние изменилось
            OnPropertyChanged(nameof(IsDetailPanelVisible));
            OnPropertyChanged(nameof(CanEditDetails));
            OnPropertyChanged(nameof(CanIssue));
            OnPropertyChanged(nameof(CanComplete));
            OnPropertyChanged(nameof(CanArchive));
}
    }
}