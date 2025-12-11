using AddonLocalizer.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AddonLocalizer.PageModels;

public partial class LocalizationDetailPageModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty]
    private LocalizationEntryViewModel? _entry;

    [ObservableProperty]
    private string _locationDetails = string.Empty;

    [ObservableProperty]
    private string _parameterDetails = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Entry", out var entry) && entry is LocalizationEntryViewModel viewModel)
        {
            Entry = viewModel;
            BuildLocationDetails();
            BuildParameterDetails();
        }
    }

    private void BuildLocationDetails()
    {
        if (Entry == null) return;

        var locations = Entry.GetLocations();
        if (locations.Count == 0)
        {
            LocationDetails = "No specific locations tracked (normal usage)";
            return;
        }

        var details = string.Join("\n", 
            locations.Take(20).Select(l => $"{l.FilePath} (Line {l.LineNumber})"));
        
        if (locations.Count > 20)
        {
            details += $"\n... and {locations.Count - 20} more locations";
        }
        
        LocationDetails = details;
    }

    private void BuildParameterDetails()
    {
        if (Entry == null) return;

        var formatParameters = Entry.GetFormatParameters();
        if (formatParameters.Count == 0)
        {
            ParameterDetails = "No format parameters";
            return;
        }

        var details = string.Join("\n",
            formatParameters.Select((p, i) => 
                $"Parameter {i + 1}: {p.Type} ({p.RawSpecifier})"));

        ParameterDetails = details;
    }

    [RelayCommand]
    private async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }
}
