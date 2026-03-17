using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
    public class AdminFuelTypesViewModel : BaseViewModel
    {
        private readonly ICarManagementService _carService;

        public ObservableCollection<FuelType> FuelTypes { get; set; } = new();

        private FuelType? _selectedFuelType;
        public FuelType? SelectedFuelType
        {
            get => _selectedFuelType;
            set { _selectedFuelType = value; OnPropertyChanged(); }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddFuelTypeCommand { get; }
        public ICommand EditFuelTypeCommand { get; }
        public ICommand DeleteFuelTypeCommand { get; }

        public AdminFuelTypesViewModel(ICarManagementService carService)
        {
            _carService = carService;

            LoadDataCommand = new RelayCommand(async _ => await LoadAsync());
            AddFuelTypeCommand = new RelayCommand(async _ => await OpenEditorAsync(null)); // null = новый
            EditFuelTypeCommand = new RelayCommand(async _ => await OpenEditorAsync(SelectedFuelType), _ => SelectedFuelType != null);
            DeleteFuelTypeCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedFuelType != null);

            LoadDataCommand.Execute(null);
        }

        private async Task LoadAsync()
        {
            var list = await _carService.GetFuelTypesAsync();
            FuelTypes = new ObservableCollection<FuelType>(list);
            OnPropertyChanged(nameof(FuelTypes));
        }

        private async Task OpenEditorAsync(FuelType? fuelTypeToEdit)
        {
            var editorVm = ServicesProvider.GetService<FuelTypeEditorViewModel>();
            if (editorVm == null) return;

            if (fuelTypeToEdit != null)
                editorVm.Initialize(fuelTypeToEdit);
            else
                editorVm.Initialize(new FuelType());

            var window = ServicesProvider.GetService<FuelTypeEditorView>();
            if (window == null) return;

            editorVm.RequestClose += (result) =>
            {
                window.DialogResult = result;
                window.Close();
            };

            window.DataContext = editorVm;

            if (window.ShowDialog() == true)
            {
                await LoadAsync();
            }
        }

        private async Task DeleteAsync()
        {
            if (MessageBox.Show($"Удалить {SelectedFuelType!.Name}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    await _carService.DeleteFuelTypeAsync(SelectedFuelType.Id);
                    await LoadAsync();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
    }
}
