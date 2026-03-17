using System;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

namespace WaybillWpf.ViewModels.Admin
{
    public class FuelTypeEditorViewModel : BaseViewModel
    {
        private readonly ICarManagementService _carService;

        private int _id;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public event Action<bool>? RequestClose;

        public ICommand SaveCommand { get; }

        public FuelTypeEditorViewModel(ICarManagementService carService)
        {
            _carService = carService;
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        }

        public void Initialize(FuelType fuelType)
        {
            _id = fuelType.Id;
            Name = fuelType.Name;
            Price = fuelType.Price;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) && Price >= 0;
        }

        private async Task SaveAsync()
        {
            var fuelType = new FuelType
            {
                Id = _id,
                Name = Name,
                Price = Price
            };

            await _carService.SaveFuelTypeAsync(fuelType);
            RequestClose?.Invoke(true);
        }
    }
}
