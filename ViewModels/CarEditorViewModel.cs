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

        public float FuelRate
        {
            get => _car.FuelRate;
            set { _car.FuelRate = value; OnPropertyChanged(); }
        }

        public event Action<bool>? RequestClose; // Событие для закрытия окна
        public ICommand SaveCommand { get; }

        public CarEditorViewModel(ICarManagementService carService)
        {
            _carService = carService;
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !string.IsNullOrWhiteSpace(Model));
        }

        public void Initialize(Car car)
        {
            // Клонируем или используем напрямую (лучше клонировать для отмены, но для простоты берем ссылку)
            // Если car существующий, мапим свойства
            _car = car;
            OnPropertyChanged(nameof(Model));
            OnPropertyChanged(nameof(FuelRate));
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