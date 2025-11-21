// В папке ViewModels/LoginViewModel.cs
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WaybillWpf.Domain.DTO;
using WaybillWpf.Domain.Enums;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels.Common; // Убедитесь, что RelayCommand здесь
using WaybillWpf.ViewModels.Base;        // Убедитесь, что BaseViewModel здесь
using WaybillWpf.Services;
using WaybillWpf.Views;
using WaybillWpf.Views.Admin;


namespace WaybillWpf.ViewModels
{
    // Важно: BaseViewModel должен содержать OnPropertyChanged, как вы и показывали.
    public class LoginViewModel : BaseViewModel 
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoggingIn;

        // Событие, на которое View подпишется, чтобы закрыть окно
        public event Action? CloseWindow;

        #region Public Properties

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value) // Проверка, чтобы не вызывать событие без надобности
                {
                    _username = value;
                    OnPropertyChanged(); // Вызываем OnPropertyChanged("Username")
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(); // Вызываем OnPropertyChanged("Password")
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(); // Вызываем OnPropertyChanged("ErrorMessage")
                }
            }
        }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                if (_isLoggingIn != value)
                {
                    _isLoggingIn = value;
                    OnPropertyChanged(); // Вызываем OnPropertyChanged("IsLoggingIn")
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoginCommand { get; }
        public ICommand OpenRegistrationCommand { get; }

        #endregion

        // Конструктор получает сервисы через DI
        public LoginViewModel(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;

            LoginCommand = new RelayCommand(async _ => await OnLoginAsync(), _ => CanLogin());
            OpenRegistrationCommand = new RelayCommand(_ => OnOpenRegistration());
        }

        private async Task OnLoginAsync()
        {
            ErrorMessage = string.Empty;
            IsLoggingIn = true;

            try
            {
                var authData = new AuthUserData
                {
                    Name = this.Username,
                    Password = this.Password
                };

                // 1. Пытаемся войти
                var user = await _authService.LoginAsync(authData);

                if (user == null)
                {
                    ErrorMessage = "Неверный логин или пароль.";
                    return;
                }

                // 2. Успех! Сохраняем пользователя
                _currentUserService.CurrentUser = user;
                Window? newWindow = null;

                switch (user.Role)
                {
                    case UserRole.Admin:
                        newWindow = ServicesProvider.GetService<AdminMainView>();
                        break;
                    case UserRole.Employee:
                        newWindow = ServicesProvider.GetService<ManagerWaybillView>();
                        break;
                }

                // 3. Сообщаем View, что можно закрываться
                newWindow?.Show();
                CloseWindow?.Invoke();

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Произошла ошибка: {ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        /// <summary>
        /// Открывает окно регистрации.
        /// </summary>
        private void OnOpenRegistration()
        {
            // Используем ваш DI-контейнер, чтобы получить НОВОЕ окно регистрации
            var registrationView = ServicesProvider.GetService<RegistrationView>();

            if (registrationView == null)
            {
                ErrorMessage = "Не удалось открыть окно регистрации.";
                return;
            }
            
            registrationView.Show();
            CloseWindow?.Invoke();
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoggingIn;
        }
    }
}