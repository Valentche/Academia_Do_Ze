using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class DashboardListPage : ContentPage
{
    public DashboardListPage(DashboardListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // OnAppearing é chamado sempre que a página aparece: recarrega os totais do dashboard.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardListViewModel viewModel)
        {
            await viewModel.LoadDashboardDataCommand.ExecuteAsync(null);
        }
    }
}
