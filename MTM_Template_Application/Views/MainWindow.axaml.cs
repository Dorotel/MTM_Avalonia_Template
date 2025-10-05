using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MTM_Template_Application.ViewModels;

namespace MTM_Template_Application.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Status bar clicked - show error details dialog
    /// </summary>
    private async void StatusBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel && viewModel.ActiveErrors != null)
        {
            // TODO: Show ConfigurationErrorDialog with error details
            // For now, just log that it was clicked
            // await ShowErrorDetailsDialog(viewModel.ActiveErrors);
        }
    }
}
