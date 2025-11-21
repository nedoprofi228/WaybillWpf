// В папке Views/
using System.Windows;
using System.Windows.Controls;
using WaybillWpf.ViewModels;

namespace WaybillWpf.Views
{
    public partial class RegistrationView : Window
    {
        // Получаем ViewModel через ваш DI (если окно создается через DI)
        public RegistrationView(RegistrationViewModel viewModel)
        {
            InitializeComponent();
            
            this.DataContext = viewModel;

            // Подписываемся на событие из ViewModel
            viewModel.CloseAction += () => this.Close();
        }

        // Этот хак нужен, чтобы передать пароль в ViewModel без нарушения MVVM
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RegistrationViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RegistrationViewModel viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}