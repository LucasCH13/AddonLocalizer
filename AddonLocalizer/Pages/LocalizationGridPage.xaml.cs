using System.Diagnostics;
using Syncfusion.Maui.DataGrid;

namespace AddonLocalizer.Pages;

public partial class LocalizationGridPage : ContentPage
{
    private readonly LocalizationGridPageModel _viewModel;
    private string? _pendingCopyValue;

    public LocalizationGridPage(LocalizationGridPageModel viewModel)
    {
        try
        {
            Debug.WriteLine("[LocalizationGridPage] Constructor started");
            InitializeComponent();
            Debug.WriteLine("[LocalizationGridPage] InitializeComponent completed");
            
            _viewModel = viewModel;
            BindingContext = viewModel;
            
            // Subscribe to grid events for better editing experience
            dataGrid.CurrentCellBeginEdit += DataGrid_CurrentCellBeginEdit;
            dataGrid.CurrentCellEndEdit += DataGrid_CurrentCellEndEdit;
            dataGrid.CellRightTapped += DataGrid_CellRightTapped;
            
            Debug.WriteLine("[LocalizationGridPage] BindingContext set");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalizationGridPage] Error in constructor: {ex.Message}");
            Debug.WriteLine($"[LocalizationGridPage] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private async void DataGrid_CellRightTapped(object? sender, DataGridCellRightTappedEventArgs e)
    {
        Debug.WriteLine($"[LocalizationGridPage] Cell right-tapped - Row: {e.RowColumnIndex.RowIndex}, Column: {e.RowColumnIndex.ColumnIndex}");
        
        // Get the cell value
        var cellValue = GetCellValue(e.RowColumnIndex.RowIndex, e.RowColumnIndex.ColumnIndex);
        if (string.IsNullOrEmpty(cellValue))
        {
            Debug.WriteLine("[LocalizationGridPage] Cell value is empty, skipping context menu");
            return;
        }

        _pendingCopyValue = cellValue;

#if WINDOWS
        ShowWindowsContextMenu();
#else
        // Fallback for other platforms
        var action = await DisplayActionSheet("Cell Options", "Cancel", null, "Copy");
        if (action == "Copy")
        {
            await CopyToClipboard(cellValue);
        }
#endif
    }

#if WINDOWS
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    private void ShowWindowsContextMenu()
    {
        if (dataGrid.Handler?.PlatformView is not Microsoft.UI.Xaml.FrameworkElement platformView)
            return;

        var menu = new Microsoft.UI.Xaml.Controls.MenuFlyout();
        
        var copyItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
        {
            Text = "Copy",
            Icon = new Microsoft.UI.Xaml.Controls.SymbolIcon(Microsoft.UI.Xaml.Controls.Symbol.Copy)
        };
        copyItem.Click += async (s, args) =>
        {
            if (_pendingCopyValue != null)
            {
                await CopyToClipboard(_pendingCopyValue);
            }
        };
        menu.Items.Add(copyItem);

        // Get cursor position in screen coordinates and convert to client coordinates
        if (GetCursorPos(out POINT cursorPos))
        {
            var window = Application.Current?.Windows[0].Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (window != null)
            {
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                
                // Convert screen coordinates to window client coordinates
                ScreenToClient(windowHandle, ref cursorPos);
                
                // Account for DPI scaling
                var scale = platformView.XamlRoot?.RasterizationScale ?? 1.0;
                var windowRelativeX = cursorPos.X / scale;
                var windowRelativeY = cursorPos.Y / scale;
                
                // Get the dataGrid's position within the window using TransformToVisual
                var transform = platformView.TransformToVisual(null);
                var gridPositionInWindow = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
                
                // Calculate position relative to the dataGrid element
                var relativeX = windowRelativeX - gridPositionInWindow.X;
                var relativeY = windowRelativeY - gridPositionInWindow.Y;
                
                menu.ShowAt(platformView, new Windows.Foundation.Point(relativeX, relativeY));
                return;
            }
        }
        
        // Fallback to showing at element center
        menu.ShowAt(platformView);
    }
#endif

    private string? GetCellValue(int rowIndex, int columnIndex)
    {
        try
        {
            // Row index 0 is the header, so data starts at index 1
            if (rowIndex < 1 || columnIndex < 0)
                return null;

            var dataRowIndex = rowIndex - 1; // Adjust for header row
            if (dataRowIndex >= _viewModel.FilteredEntries.Count)
                return null;

            var entry = _viewModel.FilteredEntries[dataRowIndex];
            var column = dataGrid.Columns[columnIndex];
            var mappingName = column.MappingName;

            // Use reflection to get the property value
            var property = entry.GetType().GetProperty(mappingName);
            var value = property?.GetValue(entry);
            
            return value?.ToString();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalizationGridPage] Error getting cell value: {ex.Message}");
            return null;
        }
    }

    private async Task CopyToClipboard(string text)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(text);
            Debug.WriteLine($"[LocalizationGridPage] Copied to clipboard: {text}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalizationGridPage] Error copying to clipboard: {ex.Message}");
            await DisplayAlert("Error", "Failed to copy to clipboard", "OK");
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private void DataGrid_CurrentCellBeginEdit(object? sender, DataGridCurrentCellBeginEditEventArgs e)
    {
        Debug.WriteLine($"[LocalizationGridPage] Cell edit started - Row: {e.RowColumnIndex.RowIndex}, Column: {e.RowColumnIndex.ColumnIndex}");
        
        // Try to apply styling to the editor after a short delay to ensure it's created
#if WINDOWS
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(50); // Small delay to let editor initialize
            try
            {
                ApplyEditorStyling();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LocalizationGridPage] Error applying editor styling: {ex.Message}");
            }
        });
#endif
    }

#if WINDOWS
    private void ApplyEditorStyling()
    {
        // Find all TextBox controls in the visual tree
        var textBoxes = FindVisualChildren<Microsoft.UI.Xaml.Controls.TextBox>(dataGrid.Handler?.PlatformView as Microsoft.UI.Xaml.FrameworkElement);
        foreach (var textBox in textBoxes)
        {
            Debug.WriteLine("[LocalizationGridPage] Found TextBox, applying styling");
            textBox.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
            textBox.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Black);
            textBox.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 230, 118));
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(Microsoft.UI.Xaml.DependencyObject? depObj) where T : Microsoft.UI.Xaml.DependencyObject
    {
        if (depObj == null) yield break;

        for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(depObj, i);
            
            if (child is T t)
            {
                yield return t;
            }

            foreach (var childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }
#endif

    private static void DataGrid_CurrentCellEndEdit(object? sender, DataGridCurrentCellEndEditEventArgs e)
    {
        // Force the grid to commit the edit
        Debug.WriteLine($"[LocalizationGridPage] Cell edit completed - Row: {e.RowColumnIndex.RowIndex}, Column: {e.RowColumnIndex.ColumnIndex}");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("[LocalizationGridPage] OnAppearing called");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Debug.WriteLine("[LocalizationGridPage] OnDisappearing called");
    }
}
