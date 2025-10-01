using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Template_Application.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
