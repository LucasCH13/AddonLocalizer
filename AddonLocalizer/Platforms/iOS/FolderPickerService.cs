using AddonLocalizer.Services;

namespace AddonLocalizer.Platforms.iOS;

public class FolderPickerService : IFolderPickerService
{
    public Task<string?> PickFolderAsync()
    {
        // iOS folder picker implementation would go here
        // For now, return null as this is primarily a desktop app
        return Task.FromResult<string?>(null);
    }
}
