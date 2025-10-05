using Avalonia.Controls;
using Serilog;

namespace MTM_Template_Application.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        Log.Debug("[MainView] Constructor - Initializing component");
        InitializeComponent();
        Log.Information("[MainView] MainView initialized successfully");

        // Log when the view is loaded
        this.Loaded += (sender, args) =>
        {
            Log.Information("[MainView] MainView loaded and displayed");
            Log.Debug("[MainView] DataContext type: {DataContextType}", DataContext?.GetType().Name ?? "null");
        };
    }
}
