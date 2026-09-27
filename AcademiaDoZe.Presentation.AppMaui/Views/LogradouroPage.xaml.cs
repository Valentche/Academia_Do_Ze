using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroPage : ContentPage
{
    public LogradouroPage(LogradouroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Ao abrir, deixa a ViewModel decidir entre cadastro (novo) e edição (Id recebido pela rota).
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LogradouroViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
