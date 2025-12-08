using AddonLocalizer.Models;
using CommunityToolkit.Mvvm.Input;

namespace AddonLocalizer.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}