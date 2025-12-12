using AddonLocalizer.PageModels;
using System.Diagnostics;

namespace AddonLocalizer.Pages;

public partial class LocalizationGridPage : ContentPage
{
    private readonly LocalizationGridPageModel _viewModel;

    public LocalizationGridPage(LocalizationGridPageModel viewModel)
    {
        try
        {
            Debug.WriteLine("[LocalizationGridPage] Constructor started");
            InitializeComponent();
            Debug.WriteLine("[LocalizationGridPage] InitializeComponent completed");
            
            _viewModel = viewModel;
            BindingContext = viewModel;
            
            Debug.WriteLine("[LocalizationGridPage] BindingContext set");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocalizationGridPage] Error in constructor: {ex.Message}");
            Debug.WriteLine($"[LocalizationGridPage] Stack trace: {ex.StackTrace}");
            throw;
        }
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
