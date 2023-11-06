using NetworkTool.WPF.ViewModels;

namespace NetworkTool.WPF.Views;

/// <summary>
///     Interaction logic for MainView.xaml
/// </summary>
public partial class MainView
{
    public MainView()
    {
        DataContext = new MainViewModel();
        InitializeComponent();
    }
}