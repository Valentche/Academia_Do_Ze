using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class MatriculaListViewModel : BaseViewModel
{
    public ObservableCollection<string> FilterTypes { get; } = ["Nome", "CPF", "Id", "Id Aluno", "Ativas", "Vencendo em dias"];
    private readonly IMatriculaService _matriculaService;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private string _selectedFilterType = "Nome";
    public string SelectedFilterType
    {
        get => _selectedFilterType;
        set => SetProperty(ref _selectedFilterType, value);
    }

    private ObservableCollection<MatriculaDto> _matriculas = [];
    public ObservableCollection<MatriculaDto> Matriculas
    {
        get => _matriculas;
        set => SetProperty(ref _matriculas, value);
    }

    private MatriculaDto? _selectedMatricula;
    public MatriculaDto? SelectedMatricula
    {
        get => _selectedMatricula;
        set => SetProperty(ref _selectedMatricula, value);
    }

    public MatriculaListViewModel(IMatriculaService matriculaService)
    {
        _matriculaService = matriculaService;
        Title = "Matriculas";
    }

    [RelayCommand]
    private async Task AddMatriculaAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("matricula");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de cadastro: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EditMatriculaAsync(MatriculaDto matricula)
    {
        try
        {
            if (matricula == null)
                return;
            await Shell.Current.GoToAsync($"matricula?Id={matricula.Id}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao navegar para tela de edição: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadMatriculasAsync();
    }

    [RelayCommand]
    private async Task SearchMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Limpa a lista atual
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();
            });

            IEnumerable<MatriculaDto> resultados = [];

            // Busca as matriculas de acordo com o filtro
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                if (SelectedFilterType == "Ativas")
                {
                    resultados = await _matriculaService.ObterAtivasAsync(0, cts.Token) ?? [];
                }
                else
                {
                    resultados = await _matriculaService.ObterTodasAsync(cts.Token) ?? [];
                }
            }
            else if (SelectedFilterType == "Nome")
            {
                var todas = await _matriculaService.ObterTodasAsync(cts.Token) ?? [];
                var termo = SearchText.Trim();
                resultados = todas.Where(m => m.AlunoMatricula?.Nome != null &&
                    m.AlunoMatricula.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase));
            }
            else if (SelectedFilterType == "CPF")
            {
                var cpfLimpo = new string([.. SearchText.Where(char.IsDigit)]);
                var todas = await _matriculaService.ObterTodasAsync(cts.Token) ?? [];
                if (cpfLimpo.Length > 0)
                {
                    resultados = todas.Where(m => m.AlunoMatricula?.Cpf != null &&
                        new string([.. m.AlunoMatricula.Cpf.Where(char.IsDigit)]).Contains(cpfLimpo));
                }
                else
                {
                    resultados = todas;
                }
            }
            else if (SelectedFilterType == "Id")
            {
                if (int.TryParse(SearchText.Trim(), out int id) && id > 0)
                {
                    var matricula = await _matriculaService.ObterPorIdAsync(id, cts.Token);
                    if (matricula != null)
                        resultados = [matricula];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID, informe um número válido.", "OK");
                }
            }
            else if (SelectedFilterType == "Id Aluno")
            {
                if (int.TryParse(SearchText.Trim(), out int alunoId) && alunoId > 0)
                {
                    resultados = await _matriculaService.ObterPorAlunoIdAsync(alunoId, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por ID do Aluno, informe um número válido.", "OK");
                }
            }
            else if (SelectedFilterType == "Ativas")
            {
                var termo = SearchText.Trim();
                if (int.TryParse(termo, out int alunoId) && alunoId > 0)
                {
                    resultados = await _matriculaService.ObterAtivasAsync(alunoId, cts.Token) ?? [];
                }
                else
                {
                    var ativas = await _matriculaService.ObterAtivasAsync(0, cts.Token) ?? [];
                    resultados = ativas.Where(m => m.AlunoMatricula?.Nome != null &&
                        m.AlunoMatricula.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase));
                }
            }
            else if (SelectedFilterType == "Vencendo em dias")
            {
                if (int.TryParse(SearchText.Trim(), out int dias))
                {
                    resultados = await _matriculaService.ObterVencendoEmDiasAsync(dias, cts.Token) ?? [];
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("Aviso", "Para busca por vencimento, informe a quantidade de dias (número inteiro).", "OK");
                }
            }
            else
            {
                var todas = await _matriculaService.ObterTodasAsync(cts.Token) ?? [];
                var termo = SearchText.Trim();
                resultados = todas.Where(m => m.AlunoMatricula?.Nome != null &&
                    m.AlunoMatricula.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase));
            }

            // Atualiza a coleção na thread principal
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var item in resultados)
                {
                    Matriculas.Add(item);
                }
                OnPropertyChanged(nameof(Matriculas));
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao buscar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadMatriculasAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Limpa a lista atual antes de carregar novos dados
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Matriculas.Clear();
                OnPropertyChanged(nameof(Matriculas));
            });

            var matriculasList = await _matriculaService.ObterTodasAsync(cts.Token);
            if (matriculasList != null)
            {
                // Garantir que a atualização da UI aconteça na thread principal
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (var matricula in matriculasList)
                    {
                        Matriculas.Add(matricula);
                    }
                    OnPropertyChanged(nameof(Matriculas));
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar matrículas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMatriculaAsync(MatriculaDto matricula)
    {
        if (matricula == null)
            return;

        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a matrícula {matricula.Id}?",
            "Sim", "Não");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            bool success = await _matriculaService.RemoverAsync(matricula.Id, cts.Token);
            if (success)
            {
                Matriculas.Remove(matricula);
                await Shell.Current.DisplayAlertAsync("Sucesso", "Matrícula excluída com sucesso!", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync("Erro", "Não foi possível excluir a matrícula.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao excluir matrícula: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
