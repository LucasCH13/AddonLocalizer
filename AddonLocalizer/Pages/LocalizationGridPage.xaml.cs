using AddonLocalizer.PageModels;

namespace AddonLocalizer.Pages;

public partial class LocalizationGridPage : ContentPage
{
    public LocalizationGridPage(LocalizationGridPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
