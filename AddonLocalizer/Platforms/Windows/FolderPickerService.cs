using Windows.Storage.Pickers;

namespace AddonLocalizer.Platforms.Windows;

public class FolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        var folderPicker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List
        };
        
        folderPicker.FileTypeFilter.Add("*");

        var hwnd = ((MauiWinUIWindow)Application.Current?.Windows[0].Handler.PlatformView!).WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
