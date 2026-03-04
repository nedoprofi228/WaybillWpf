
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Base; // Убедись, что тут лежит RelayCommand
using WaybillWpf.ViewModels.Common;
using WaybillWpf.Views;

namespace WaybillWpf.ViewModels.Admin
{
    public class DriverRegistrationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;
        
        private Driver _driver;

        // --- Свойства водителя ---
        public string DriverName
        {
            get => _driver.FullName;
            set { _driver.FullName = value; OnPropertyChanged(); }
        }

        public string Login
        {
            get => _driver.Login;
            set { _driver.Login = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _driver.Password;
            set { _driver.Password = value; OnPropertyChanged(); }
        }

        // --- Свойства прав (Proxy) ---
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

        // --- События для управления окнами (View подпишется на них) ---
        public event Action? CloseAction;     // Успешная регистрация

        // --- Команды ---
        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }
        public ICommand NavigateToEmployeeCommand { get; }

        public DriverRegistrationViewModel(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
            
            // Инициализируем нового водителя сразу с пустыми правами
            _driver = new Driver
            {
                Role = UserRole.Driver,
                DriveLicense = new DriveLicense
                {
                    IssueDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                    ExpirationDate = DateTime.SpecifyKind(DateTime.Now.AddYears(10), DateTimeKind.Utc)
                }
            };

            // Инициализация команд
            RegisterCommand = new RelayCommand(async _ => await RegisterAsync(), _ => Validate());
            
            NavigateToLoginCommand = new RelayCommand(_ => OpenLoginRequested());
            NavigateToEmployeeCommand = new RelayCommand(_ => OpenEmployeeRegistrationRequested());
        }

        private void EnsureLicenseExists()
        {
            if (_driver.DriveLicense == null) 
                _driver.DriveLicense = new DriveLicense();
        }

        private bool Validate()
        {
            return !string.IsNullOrWhiteSpace(DriverName) &&
                   !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(LicenseNumber);
        }

        private async Task RegisterAsync()
        {
            try 
            {
                // Здесь можно добавить хэширование пароля перед отправкой, если сервис этого не делает
                var user = await _authService.RegisterAsync(_driver);
                _currentUserService.CurrentUser = user;

                var window = ServicesProvider.GetService<DriverManagementView>();
                window.Show();
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                // Тут можно добавить вывод ошибки через MessageBox или свойство ErrorMessage
                System.Diagnostics.Debug.WriteLine($"Ошибка регистрации: {ex.Message}");
            }
        }

        private void OpenLoginRequested()
        {
            LoginView window = ServicesProvider.GetService<LoginView>();
            window.ShowDialog();
            CloseAction?.Invoke();
        }

        private void OpenEmployeeRegistrationRequested()
        {
            RegistrationView window = ServicesProvider.GetService<RegistrationView>();
            window.ShowDialog();
            CloseAction?.Invoke();
        }
        
        
    }
}