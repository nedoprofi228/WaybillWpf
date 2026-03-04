using System.Windows;
using WaybillWpf.ViewModels;
using WaybillWpf.ViewModels.Admin;
using WaybillWpf.Views.Admin;

namespace WaybillWpf.Views;

public partial class DriverManagementView : Window
{
    public DriverManagementView(DriverManagementViewModel driverManagementViewModel)
    {
        InitializeComponent();
        driverManagementViewModel.CloseAction += () => this.Close();
        DataContext = driverManagementViewModel;
    }
}