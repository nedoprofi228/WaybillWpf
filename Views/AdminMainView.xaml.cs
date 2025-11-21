using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.Services;
using WaybillWpf.ViewModels.Admin;

namespace WaybillWpf.Views.Admin
{
    /// <summary>
    /// Interaction logic for AdminMainView.xaml
    /// </summary>
    public partial class AdminMainView : Window
    {
        public AdminMainView(AdminMainViewModel viewModel, ICurrentUserService currentUserService)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Subscribe to the logout event
            viewModel.RequestLogOut += () =>
            {
                currentUserService.Logout();
                this.Close();
            };
        }
    }
}