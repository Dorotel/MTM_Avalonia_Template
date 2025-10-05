using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Template_Application.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private ObservableCollection<string> _bootLogs = new();

    [ObservableProperty]
    private bool _hasBootLogs;

    [ObservableProperty]
    private int _bootDurationMs;

    [ObservableProperty]
    private double _bootMemoryMB;

    /// <summary>
    /// Set boot logs from the completed boot sequence.
    /// </summary>
    public void SetBootLogs(IEnumerable<string> logs, int durationMs = 0, double memoryMB = 0)
    {
        BootLogs.Clear();
        foreach (var log in logs)
        {
            BootLogs.Add(log);
        }
        HasBootLogs = BootLogs.Count > 0;
        BootDurationMs = durationMs;
        BootMemoryMB = memoryMB;
    }
}
