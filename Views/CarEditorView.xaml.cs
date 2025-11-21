using System.Windows;
using WaybillWpf.ViewModels.Admin;

namespace WaybillWpf.Views.Admin;

public partial class CarEditorView : Window
{
    public CarEditorView(CarEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.RequestClose += (result) => this.Close();
    }
}