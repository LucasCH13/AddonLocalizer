using AddonLocalizer.Core.Interfaces;
using AddonLocalizer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AddonLocalizer.PageModels;

public partial class LocalizationHomePageModel : ObservableObject
{
    private readonly IFolderPickerService _folderPickerService;
    private readonly ILuaLocalizationParserService _parserService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private string? _selectedDirectory;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Select a directory to begin";

    [ObservableProperty]
    private int _totalEntries;

    [ObservableProperty]
    private int _concatenatedEntries;

    [ObservableProperty]
    private int _stringFormatEntries;

    [ObservableProperty]
    private bool _hasData;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _canCancel;

    public LocalizationHomePageModel(
        IFolderPickerService folderPickerService,
        ILuaLocalizationParserService parserService)
    {
        _folderPickerService = folderPickerService;
        _parserService = parserService;

        LoadRecentDirectory();
    }

    private void LoadRecentDirectory()
    {
        if (Preferences.Default.ContainsKey("last_directory"))
        {
            SelectedDirectory = Preferences.Default.Get("last_directory", string.Empty);
        }
    }

    [RelayCommand]
    private async Task SelectDirectory()
    {
        var directory = await _folderPickerService.PickFolderAsync();
        
        if (!string.IsNullOrWhiteSpace(directory))
        {
            SelectedDirectory = directory;
            Preferences.Default.Set("last_directory", directory);
            StatusMessage = $"Directory selected: {directory}";
        }
    }

    [RelayCommand]
    private async Task ParseDirectory()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory))
        {
            StatusMessage = "Please select a directory first";
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsLoading = true;
            CanCancel = true;
            Progress = 0;
            StatusMessage = "Parsing localization files...";

            var result = await Task.Run(() =>
            {
                try
                {
                    return _parserService.ParseDirectory(SelectedDirectory);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }, _cancellationTokenSource.Token);

            if (result == null)
            {
                StatusMessage = "Parsing cancelled";
                return;
            }

            TotalEntries = result.GlueStrings.Count;
            ConcatenatedEntries = result.Concatenated.Count();
            StringFormatEntries = result.WithStringFormat.Count();
            HasData = TotalEntries > 0;

            StatusMessage = $"Parsed {TotalEntries} localization entries";

            await Shell.Current.GoToAsync("localizations", new Dictionary<string, object>
            {
                { "ParseResult", result }
            });
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Parsing cancelled by user";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            CanCancel = false;
            Progress = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand]
    private void CancelParsing()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Cancelling...";
    }

    [RelayCommand]
    private void ClearDirectory()
    {
        SelectedDirectory = null;
        Preferences.Default.Remove("last_directory");
        StatusMessage = "Select a directory to begin";
        HasData = false;
        TotalEntries = 0;
        ConcatenatedEntries = 0;
        StringFormatEntries = 0;
    }
}
