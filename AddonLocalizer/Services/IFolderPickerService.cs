namespace AddonLocalizer.Services;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync();
}
