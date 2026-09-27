using AcademiaDoZe.Presentation.AppMaui.ViewModels;
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class LogradouroListPage : ContentPage
{
    public LogradouroListPage(LogradouroListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    // Ao abrir a página, carrega a lista de logradouros.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is LogradouroListViewModel viewModel)
        {
            await viewModel.LoadLogradourosCommand.ExecuteAsync(null);
        }
    }

    // O botão "Editar" de cada item repassa o LogradouroDto (via BindingContext do botão) para o comando da ViewModel.
    private async void OnEditButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is LogradouroDto logradouro && BindingContext is LogradouroListViewModel viewModel)
            {
                await viewModel.EditLogradouroCommand.ExecuteAsync(logradouro);
            }
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", $"Erro ao editar logradouro: {ex.Message}", "OK"); }
    }

    // O botão "Excluir" de cada item repassa o LogradouroDto para o comando de exclusão da ViewModel.
    private async void OnDeleteButtonClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is LogradouroDto logradouro && BindingContext is LogradouroListViewModel viewModel)
            {
                await viewModel.DeleteLogradouroCommand.ExecuteAsync(logradouro);
            }
        }
        catch (Exception ex) { await DisplayAlertAsync("Erro", $"Erro ao excluir logradouro: {ex.Message}", "OK"); }
    }
}
