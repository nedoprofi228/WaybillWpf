using System.Windows;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

namespace WaybillWpf.ViewModels.Admin
{
    public class CarEditorViewModel : BaseViewModel
    {
        private readonly ICarManagementService _carService;
        private Car _car = new();

        public string Model
        {
            get => _car.Model;
            set { _car.Model = value; OnPropertyChanged(); }
        }
        public string CarNumber
        {
            get => _car.CarNumber;
            set { _car.CarNumber = value; OnPropertyChanged(); }
        }

        public float FuelRate
        {
            get => _car.FuelRate;
            set { _car.FuelRate = value; OnPropertyChanged(); }
        }

        private List<FuelType> _fuelTypes = new();
        public List<FuelType> FuelTypes
        {
            get => _fuelTypes;
            set { _fuelTypes = value; OnPropertyChanged(); }
        }

        private FuelType? _selectedFuelType;
        public FuelType? SelectedFuelType
        {
            get => _selectedFuelType;
            set
            {
                _selectedFuelType = value;
                _car.FuelTypeId = value?.Id;
                OnPropertyChanged();
            }
        }

        public event Action<bool>? RequestClose; // Событие для закрытия окна
        public ICommand SaveCommand { get; set; }

        public CarEditorViewModel(ICarManagementService carService)
        {
            _carService = carService;
            
        }

        public void Initialize(Car car, List<FuelType> availableFuelTypes)
        {
            // Клонируем или используем напрямую (лучше клонировать для отмены, но для простоты берем ссылку)
            // Если car существующий, мапим свойства
            _car = car;
            FuelTypes = availableFuelTypes;
            SelectedFuelType = availableFuelTypes.FirstOrDefault(f => f.Id == car.FuelTypeId);
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !string.IsNullOrWhiteSpace(Model));

            OnPropertyChanged(nameof(Model));
            OnPropertyChanged(nameof(FuelRate));
            OnPropertyChanged(nameof(CarNumber));
            OnPropertyChanged(nameof(SelectedFuelType));
        }

        private async Task SaveAsync()
        {
            try
            {
                await _carService.SaveCarAsync(_car);
                RequestClose?.Invoke(true); // Закрываем с DialogResult = true
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }
    }
}