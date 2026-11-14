using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ControleAcessoPage : ContentPage
{
    private readonly ControleAcessoViewModel _viewModel;

    public ControleAcessoPage(ControleAcessoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarDadosIniciaisCommand.ExecuteAsync(null);
    }
}
