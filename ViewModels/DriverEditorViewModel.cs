// ViewModels/Admin/AdminDriversViewModel.cs
// (Структура 1 в 1 как у AdminCarsViewModel, только вызывает _driverService и DriverEditorViewModel)

// ViewModels/Admin/DriverEditorViewModel.cs

using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

namespace WaybillWpf.ViewModels.Admin
{
    public class DriverEditorViewModel : BaseViewModel
    {
        private readonly IDriverManagementService _driverService;
        private Driver _driver = new();

        // Свойства водителя
        public string DriverName
        {
            get => _driver.DriverName;
            set { _driver.DriverName = value; OnPropertyChanged(); }
        }

        // Свойства прав (Proxy свойства для UI)
        public string LicenseNumber
        {
            get => _driver.DriveLicense?.LicenseNumber ?? "";
            set 
            {
                EnsureLicenseExists();
                _driver.DriveLicense!.LicenseNumber = value; 
                OnPropertyChanged(); 
            }
        }
        
        public DateTime LicenseIssueDate
        {
            get => _driver.DriveLicense?.IssueDate ?? DateTime.Now;
            set 
            {
                EnsureLicenseExists();
                _driver.DriveLicense!.IssueDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
                OnPropertyChanged(); 
            }
        }

        public DateTime LicenseExpirationDate
        {
            get => _driver.DriveLicense?.ExpirationDate ?? DateTime.Now.AddYears(10);
            set 
            {
                EnsureLicenseExists();
                
                _driver.DriveLicense!.ExpirationDate = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
                OnPropertyChanged(); 
            }
        }

        public event Action<bool>? RequestClose;
        public ICommand SaveCommand { get; }

        public DriverEditorViewModel(IDriverManagementService driverService)
        {
            _driverService = driverService;
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => Validate());
        }

        public void Initialize(Driver driver)
        {
            _driver = driver;
            if(_driver.DriveLicense == null)
            {
                _driver.DriveLicense = new DriveLicense { IssueDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc), ExpirationDate =DateTime.SpecifyKind(DateTime.Now.AddYears(10), DateTimeKind.Utc) };
            }
            // Уведомляем UI об обновлении всех полей
            OnPropertyChanged(string.Empty); 
        }

        private void EnsureLicenseExists()
        {
            if (_driver.DriveLicense == null) _driver.DriveLicense = new DriveLicense();
        }

        private bool Validate() => !string.IsNullOrWhiteSpace(DriverName) && 
                                   !string.IsNullOrWhiteSpace(LicenseNumber);

        private async Task SaveAsync()
        {
            await _driverService.SaveDriverAsync(_driver);
            RequestClose?.Invoke(true);
        }
    }
}