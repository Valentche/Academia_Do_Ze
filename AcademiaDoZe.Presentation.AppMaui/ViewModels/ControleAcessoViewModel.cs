using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class ControleAcessoViewModel : BaseViewModel
{
    private readonly IAcessoService _acessoService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;

    private List<AlunoDto> _todosAlunos = [];
    private List<ColaboradorDto> _todosColaboradores = [];

    [ObservableProperty]
    public partial bool ModoAluno { get; set; } = true;

    [ObservableProperty]
    public partial bool ModoColaborador { get; set; } = false;

    public string TextoModoSelecionado => ModoAluno ? "Modo atual selecionado: Aluno" : "Modo atual selecionado: Colaborador";

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public ObservableCollection<AlunoDto> AlunosFiltrados { get; } = [];
    public ObservableCollection<ColaboradorDto> ColaboradoresFiltrados { get; } = [];

    [ObservableProperty]
    public partial AlunoDto? SelectedAluno { get; set; }

    [ObservableProperty]
    public partial ColaboradorDto? SelectedColaborador { get; set; }

    [ObservableProperty]
    public partial string PessoaNome { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PessoaCpf { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ArquivoDto? PessoaFoto { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusTexto))]
    [NotifyPropertyChangedFor(nameof(StatusBadgeBgColor))]
    [NotifyPropertyChangedFor(nameof(BotaoAcaoTexto))]
    [NotifyPropertyChangedFor(nameof(BotaoAcaoIcone))]
    public partial bool EstaNaAcademia { get; set; }

    public string StatusTexto => EstaNaAcademia ? "DENTRO DA ACADEMIA" : "FORA DA ACADEMIA";
    public Color StatusBadgeBgColor => EstaNaAcademia ? Color.FromArgb("#4CAF50") : Color.FromArgb("#616161");
    public string BotaoAcaoTexto => EstaNaAcademia ? "Registrar Saída" : "Registrar Entrada";
    public string BotaoAcaoIcone => EstaNaAcademia ? "\ue898" : "\ue897";

    [ObservableProperty]
    public partial bool TemPessoaSelecionada { get; set; }

    // Feedback Banner
    [ObservableProperty]
    public partial bool TemFeedback { get; set; }

    [ObservableProperty]
    public partial string FeedbackTitulo { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FeedbackMensagem { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FeedbackBgColor))]
    [NotifyPropertyChangedFor(nameof(FeedbackIcone))]
    public partial bool FeedbackSucesso { get; set; } = true;

    public Color FeedbackBgColor => FeedbackSucesso ? Color.FromArgb("#1B5E20") : Color.FromArgb("#880E4F");
    public string FeedbackIcone => FeedbackSucesso ? "\ue86c" : "\ue000";

    public ObservableCollection<AcessoHistoricoItemDto> AcessosHoje { get; } = [];

    public ControleAcessoViewModel(
        IAcessoService acessoService,
        IAlunoService alunoService,
        IColaboradorService colaboradorService)
    {
        _acessoService = acessoService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        Title = "Controle de Acesso";
    }

    [RelayCommand]
    private async Task SelecionarModoAlunoAsync()
    {
        ModoAluno = true;
        ModoColaborador = false;
        OnPropertyChanged(nameof(TextoModoSelecionado));
        SearchText = string.Empty;
        SelectedColaborador = null;
        LimparPessoaSelecionada();
        FiltrarAlunos();
    }

    [RelayCommand]
    private async Task SelecionarModoColaboradorAsync()
    {
        ModoAluno = false;
        ModoColaborador = true;
        OnPropertyChanged(nameof(TextoModoSelecionado));
        SearchText = string.Empty;
        SelectedAluno = null;
        LimparPessoaSelecionada();
        FiltrarColaboradores();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (ModoAluno)
            FiltrarAlunos();
        else
            FiltrarColaboradores();
    }

    partial void OnSelectedAlunoChanged(AlunoDto? value)
    {
        if (value != null)
        {
            _ = CarregarDadosAlunoSelecionadoAsync(value);
        }
        else if (ModoAluno)
        {
            LimparPessoaSelecionada();
        }
    }

    partial void OnSelectedColaboradorChanged(ColaboradorDto? value)
    {
        if (value != null)
        {
            _ = CarregarDadosColaboradorSelecionadoAsync(value);
        }
        else if (ModoColaborador)
        {
            LimparPessoaSelecionada();
        }
    }

    private async Task CarregarDadosAlunoSelecionadoAsync(AlunoDto aluno)
    {
        PessoaNome = aluno.Nome;
        PessoaCpf = aluno.Cpf;
        PessoaFoto = aluno.Foto;
        EstaNaAcademia = await _acessoService.AlunoEstaNaAcademiaAsync(aluno.Id);
        TemPessoaSelecionada = true;
        TemFeedback = false;
    }

    private async Task CarregarDadosColaboradorSelecionadoAsync(ColaboradorDto colaborador)
    {
        PessoaNome = colaborador.Nome;
        PessoaCpf = colaborador.Cpf;
        PessoaFoto = colaborador.Foto;
        EstaNaAcademia = await _acessoService.ColaboradorEstaNaAcademiaAsync(colaborador.Id);
        TemPessoaSelecionada = true;
        TemFeedback = false;
    }

    private void LimparPessoaSelecionada()
    {
        PessoaNome = string.Empty;
        PessoaCpf = string.Empty;
        PessoaFoto = null;
        EstaNaAcademia = false;
        TemPessoaSelecionada = false;
        TemFeedback = false;
    }

    private void FiltrarAlunos()
    {
        AlunosFiltrados.Clear();
        var termo = SearchText.Trim();
        var lista = string.IsNullOrWhiteSpace(termo)
            ? _todosAlunos
            : _todosAlunos.Where(a =>
                a.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                a.Cpf.Contains(termo) ||
                a.Id.ToString() == termo).ToList();

        foreach (var aluno in lista)
        {
            AlunosFiltrados.Add(aluno);
        }

        if (!string.IsNullOrWhiteSpace(termo) && lista.Count == 1)
        {
            SelectedAluno = lista[0];
        }
    }

    private void FiltrarColaboradores()
    {
        ColaboradoresFiltrados.Clear();
        var termo = SearchText.Trim();
        var lista = string.IsNullOrWhiteSpace(termo)
            ? _todosColaboradores
            : _todosColaboradores.Where(c =>
                c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                c.Cpf.Contains(termo) ||
                c.Id.ToString() == termo).ToList();

        foreach (var colab in lista)
        {
            ColaboradoresFiltrados.Add(colab);
        }

        if (!string.IsNullOrWhiteSpace(termo) && lista.Count == 1)
        {
            SelectedColaborador = lista[0];
        }
    }

    [RelayCommand]
    private async Task RegistrarAcessoAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            AcessoResultadoDto resultado;

            if (ModoAluno)
            {
                if (SelectedAluno == null)
                {
                    await Shell.Current.DisplayAlertAsync("Atenção", "Selecione um aluno para registrar o acesso.", "OK");
                    return;
                }

                resultado = await _acessoService.RegistrarAcessoAlunoAsync(SelectedAluno.Id);
                EstaNaAcademia = await _acessoService.AlunoEstaNaAcademiaAsync(SelectedAluno.Id);
            }
            else
            {
                if (SelectedColaborador == null)
                {
                    await Shell.Current.DisplayAlertAsync("Atenção", "Selecione um colaborador para registrar o acesso.", "OK");
                    return;
                }

                resultado = await _acessoService.RegistrarAcessoColaboradorAsync(SelectedColaborador.Id);
                EstaNaAcademia = await _acessoService.ColaboradorEstaNaAcademiaAsync(SelectedColaborador.Id);
            }

            FeedbackTitulo = resultado.Titulo;
            FeedbackMensagem = resultado.Mensagem;
            FeedbackSucesso = resultado.Sucesso;
            TemFeedback = true;

            await CarregarHistoricoAcessosAsync();
        }
        catch (Exception ex)
        {
            FeedbackTitulo = "Erro";
            FeedbackMensagem = $"Erro ao registrar acesso: {ex.Message}";
            FeedbackSucesso = false;
            TemFeedback = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CarregarDadosIniciaisAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var alunos = await _alunoService.ObterTodosAsync(cts.Token);
            _todosAlunos = alunos?.ToList() ?? [];

            var colaboradores = await _colaboradorService.ObterTodosAsync(cts.Token);
            _todosColaboradores = colaboradores?.ToList() ?? [];

            if (ModoAluno)
                FiltrarAlunos();
            else
                FiltrarColaboradores();

            await CarregarHistoricoAcessosAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CarregarHistoricoAcessosAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var historico = await _acessoService.ObterAcessosHojeAsync(cts.Token);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AcessosHoje.Clear();
                foreach (var item in historico)
                {
                    AcessosHoje.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar histórico: {ex.Message}");
        }
    }
}
