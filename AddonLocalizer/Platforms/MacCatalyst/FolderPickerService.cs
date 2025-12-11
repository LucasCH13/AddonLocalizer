using AddonLocalizer.Services;

namespace AddonLocalizer.Platforms.MacCatalyst;

public class FolderPickerService : IFolderPickerService
{
    public Task<string?> PickFolderAsync()
    {
        // MacCatalyst folder picker implementation would go here
        // For now, return null as this is primarily a desktop app
        return Task.FromResult<string?>(null);
    }
}
