using System.Windows;
using WaybillWpf.ViewModels;

namespace WaybillWpf.Views;

public partial class NewWaybillView : Window
{
    public NewWaybillView(NewWaybillViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += (result) =>
        {
            DialogResult = result;
            Close();
        };
    }
}