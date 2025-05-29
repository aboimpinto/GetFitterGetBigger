namespace Olimpo.NavigationManager;

public interface INavigatableView
{
    ViewModelBase CurrentOperation { get; set; }

    bool WithBackTransaction { get; set; }
}