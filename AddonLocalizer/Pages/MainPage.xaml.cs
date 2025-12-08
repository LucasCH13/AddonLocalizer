using AddonLocalizer.Models;
using AddonLocalizer.PageModels;

namespace AddonLocalizer.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}