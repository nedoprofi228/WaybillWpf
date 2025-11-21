using System.Windows.Input;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;
using WaybillWpf.Views;

namespace WaybillWpf.ViewModels.Admin
{
    public class AdminMainViewModel : BaseViewModel
    {
        // Текущая активная VM (Машины, Водители или Статистика)
        private BaseViewModel? _currentView;
        public BaseViewModel? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        // Команды меню
        public ICommand ShowCarsCommand { get; }
        public ICommand ShowDriversCommand { get; }
        public ICommand ShowStatsCommand { get; }
        public ICommand LogoutCommand { get; }
        
        public event Action? RequestLogOut;

        public AdminMainViewModel()
        {
            // При нажатии создаем/достаем нужную VM из контейнера
            ShowCarsCommand = new RelayCommand(_ => CurrentView = ServicesProvider.GetService<AdminCarsViewModel>());
            ShowDriversCommand = new RelayCommand(_ => CurrentView = ServicesProvider.GetService<AdminDriversViewModel>());
            ShowStatsCommand = new RelayCommand(_ => CurrentView = ServicesProvider.GetService<AdminStatisticsViewModel>());

            LogoutCommand = new RelayCommand(_ => { 
                ServicesProvider.GetService<CurrentUserService>()?.Logout();
                ServicesProvider.GetService<LoginView>()?.Show();
                RequestLogOut?.Invoke();
            });
            CurrentView = ServicesProvider.GetService<AdminCarsViewModel>();
        }
    }
}