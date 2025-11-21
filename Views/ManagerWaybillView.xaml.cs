using System.Windows;
using System.Windows.Controls;
using WaybillWpf.ViewModels;

namespace WaybillWpf.Views;

public partial class ManagerWaybillView : Window
{
    public ManagerWaybillView(ManagerWaybillViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}