using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WaybillWpf.Domain.DTO; // Предполагаем, что DTO здесь
using WaybillWpf.Domain.Entities;
using WaybillWpf.Services; // Для IDriverManagementService
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;
using WaybillWpf.Views.Admin; // Пространство имен для View

namespace WaybillWpf.ViewModels.Admin
{
    public class AdminDriversViewModel : BaseViewModel
    {
        private readonly IDriverManagementService _driverService;

        // Список DTO для отображения в таблице (облегченная версия)
        private ObservableCollection<DriverDto> _drivers = new();
        public ObservableCollection<DriverDto> Drivers
        {
            get => _drivers;
            set
            {
                _drivers = value;
                OnPropertyChanged();
            }
        }

        // Выбранный элемент в таблице
        private DriverDto? _selectedDriverDto;
        public DriverDto? SelectedDriverDto
        {
            get => _selectedDriverDto;
            set
            {
                _selectedDriverDto = value;
                OnPropertyChanged();
                // Обновляем состояние кнопок, зависящих от выделения
                (EditDriverCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteDriverCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddDriverCommand { get; }
        public ICommand EditDriverCommand { get; }
        public ICommand DeleteDriverCommand { get; }

        public AdminDriversViewModel(IDriverManagementService driverService)
        {
            _driverService = driverService;

            LoadDataCommand = new RelayCommand(async _ => await LoadAsync());
            AddDriverCommand = new RelayCommand(async _ => await OnAddAsync());
            
            // Редактировать/Удалять можно только если выбран водитель
            EditDriverCommand = new RelayCommand(async _ => await OnEditAsync(), _ => SelectedDriverDto != null);
            DeleteDriverCommand = new RelayCommand(async _ => await OnDeleteAsync(), _ => SelectedDriverDto != null);

            // Загрузка при старте
            LoadDataCommand.Execute(null);
        }

        private async Task LoadAsync()
        {
            try
            {
                var list = await _driverService.GetAllDriversAsync();
                Drivers = new ObservableCollection<DriverDto>(list);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async Task OnAddAsync()
        {
            // При добавлении передаем пустую сущность Driver
            await OpenEditorAsync(new Driver());
        }

        private async Task OnEditAsync()
        {
            if (SelectedDriverDto == null) return;

            try
            {
                // Загружаем полную сущность из БД (с лицензией) для редактирования
                var fullDriver = await _driverService.GetDriverById(SelectedDriverDto.DriverId);
                await OpenEditorAsync(fullDriver);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных водителя: {ex.Message}");
            }
        }

        private async Task OpenEditorAsync(Driver driver)
        {
            // 1. Получаем VM редактора
            var editorVm = ServicesProvider.GetService<DriverEditorViewModel>();
            if (editorVm == null) return;

            editorVm.Initialize(driver);

            // 2. Создаем View и привязываем контекст
            var window = ServicesProvider.GetService<DriverEditorView>(); 
            window.DataContext = editorVm;

            // Подписываемся на закрытие окна из VM
            editorVm.RequestClose += (result) => window.Close();

            // 3. Показываем окно
            if (window.ShowDialog() == true)
            {
                // Если сохранили успешно - обновляем список
                await LoadAsync();
            }
        }

        private async Task OnDeleteAsync()
        {
            if (SelectedDriverDto == null) return;

            var res = MessageBox.Show($"Вы уверены, что хотите удалить водителя {SelectedDriverDto.DriverName}?", 
                                      "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = await _driverService.DeleteDriverAsync(SelectedDriverDto.DriverId);
                    if (success)
                    {
                        await LoadAsync();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить водителя (возможно, он занят в рейсе).");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}