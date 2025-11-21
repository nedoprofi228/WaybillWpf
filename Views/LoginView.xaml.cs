// В папке Views/LoginView.xaml.cs
using System.Windows;
using System.Windows.Controls;
using WaybillWpf.ViewModels;

namespace WaybillWpf.Views
{
    public partial class LoginView : Window
    {
        // Конструктор принимает ViewModel, которую ему передаст DI
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            
            this.DataContext = viewModel;

            // 1. Подписываемся на событие из ViewModel
            viewModel.CloseWindow += () => this.Close();
        }

        // 2. Хак для передачи пароля в ViewModel
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}