// ViewModels/Admin/AdminCarsViewModel.cs
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;
using WaybillWpf.Views.Admin;

namespace WaybillWpf.ViewModels.Admin
{
    public class AdminCarsViewModel : BaseViewModel
    {
        private readonly ICarManagementService _carService;

        public ObservableCollection<Car> Cars { get; set; } = new();

        private Car? _selectedCar;
        public Car? SelectedCar
        {
            get => _selectedCar;
            set { _selectedCar = value; OnPropertyChanged(); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddCarCommand { get; }
        public ICommand EditCarCommand { get; }
        public ICommand DeleteCarCommand { get; }

        public AdminCarsViewModel(ICarManagementService carService)
        {
            _carService = carService;

            LoadDataCommand = new RelayCommand(async _ => await LoadAsync());
            AddCarCommand = new RelayCommand(async _ => await OpenEditorAsync(null)); // null = новая
            EditCarCommand = new RelayCommand(async _ => await OpenEditorAsync(SelectedCar), _ => SelectedCar != null);
            DeleteCarCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedCar != null);

            LoadDataCommand.Execute(null);
        }

        private async Task LoadAsync()
        {
            var list = await _carService.GetAllCarsAsync();
            Cars = new ObservableCollection<Car>(list);
            OnPropertyChanged(nameof(Cars));
        }

        private async Task OpenEditorAsync(Car? carToEdit)
        {
            // Получаем VM редактора из DI
            var editorVm = ServicesProvider.GetService<CarEditorViewModel>();
            if (editorVm == null) return;

            // Если редактируем - передаем данные, если добавляем - будет пустой
            if (carToEdit != null) 
                editorVm.Initialize(carToEdit); 
            else 
                editorVm.Initialize(new Car());

            // Открываем окно (View должно быть привязано к этой VM)
            var window = ServicesProvider.GetService<CarEditorView>(); // View нужно создать
            editorVm.RequestClose += (result) =>
            {
                window.DialogResult = result;
                window.Close();
            };
            
            window.DataContext = editorVm;
            
            if (window.ShowDialog() == true)
            {
                await LoadAsync(); // Обновляем список после сохранения
            }
        }

        private async Task DeleteAsync()
        {
            if (MessageBox.Show($"Удалить {SelectedCar!.Model}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try 
                {
                    await _carService.DeleteCarAsync(SelectedCar.Id);
                    await LoadAsync();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
    }
}

// ViewModels/Admin/CarEditorViewModel.cs
