// В папке ViewModels/
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WaybillWpf.Domain.DTO; // Для AuthUserData
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels.Common; // Для RelayCommand
using WaybillWpf.Services; // Для ServicesProvider
using WaybillWpf.ViewModels.Base;
using WaybillWpf.Views;

namespace WaybillWpf.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isRegistering;

        // Событие, на которое подпишется View, чтобы закрыть окно
        public event Action? CloseAction;

        #region Public Properties

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        // ⚠️ ВАЖНО: Хранение пароля в string небезопасно. 
        // Для курсового проекта это допустимо, но в проде используют SecureString.
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                _isRegistering = value;
                OnPropertyChanged(nameof(IsRegistering));
            }
        }

        #endregion

        #region Commands

        public ICommand RegisterCommand { get; }
        public ICommand ToLoginCommand { get; }

        #endregion

        public RegistrationViewModel(IAuthService authService, ICurrentUserService currentUserService)
        {
            // Получаем сервис из вашего DI
            _authService = authService;
            _currentUserService = currentUserService;
            
            RegisterCommand = new RelayCommand(async _ => await OnRegisterAsync(), _ => CanRegister());
            ToLoginCommand = new RelayCommand(_ => ToLogin());
        }

        private async Task OnRegisterAsync()
        {
            ErrorMessage = string.Empty;
            if (!ValidateInput()) return;

            IsRegistering = true;
            try
            {
                var authData = new AuthUserData
                {
                    Name = this.Username,
                    Password = this.Password
                };
                
                // Используем ваш AuthService
                var newUser = await _authService.RegisterAsync(authData);

                if (newUser != null)
                {
                    _currentUserService.CurrentUser = newUser;
                    var managerWaybillWindow = ServicesProvider.GetService<ManagerWaybillView>();
                    managerWaybillWindow?.Show();
                    CloseAction?.Invoke(); 
                }
                else
                {
                    // (Исходя из вашей реализации RegisterAsync)
                    ErrorMessage = "Пользователь с таким именем уже существует.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Произошла ошибка: {ex.Message}";
            }
            finally
            {
                IsRegistering = false;
            }
        }

        private void ToLogin()
        {
            var loginView = ServicesProvider.GetService<LoginView>();
            loginView?.Show();
            CloseAction?.Invoke();
        }
        
        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !IsRegistering;
        }

        private bool ValidateInput()
        {
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают.";
                return false;
            }
            return true;
        }
    }
}