using NetworkTool.WPF.ViewModel;

namespace NetworkTool.WPF.View;

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