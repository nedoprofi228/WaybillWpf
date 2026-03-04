using System.Windows;
using WaybillWpf.ViewModels.Admin; // Assuming DriverEditorViewModel is in this namespace

namespace WaybillWpf.Views.Admin;

public partial class DriverRegistrationView : Window
{
    public DriverRegistrationView(DriverRegistrationViewModel vm)
    {
        InitializeComponent();

        vm.CloseAction += () => this.Close();
        
        DataContext = vm;
    }
}