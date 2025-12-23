namespace AddonLocalizer.Pages;

public partial class LocalizationDetailPage : ContentPage
{
    public LocalizationDetailPage(LocalizationDetailPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
