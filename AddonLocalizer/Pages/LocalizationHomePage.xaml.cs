using AddonLocalizer.PageModels;

namespace AddonLocalizer.Pages;

public partial class LocalizationHomePage : ContentPage
{
    public LocalizationHomePage(LocalizationHomePageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
