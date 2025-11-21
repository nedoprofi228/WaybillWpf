using System.Configuration;
using System.Data;
using System.Windows;
using WaybillWpf.Services;
using WaybillWpf.Views;

namespace WaybillWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        var window = ServicesProvider.GetService<LoginView>();
        window.Show();
    }
    
}