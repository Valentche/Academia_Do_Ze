using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Application.Services;

public class AcessoService : IAcessoService
{
    private readonly Func<IAcessoAlunoRepository> _acessoAlunoRepoFactory;
    private readonly Func<IAcessoColaboradorRepository> _acessoColaboradorRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;
    private readonly Func<IMatriculaRepository> _matriculaRepoFactory;

    public AcessoService(
        Func<IAcessoAlunoRepository> acessoAlunoRepoFactory,
        Func<IAcessoColaboradorRepository> acessoColaboradorRepoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory,
        Func<IMatriculaRepository> matriculaRepoFactory)
    {
        _acessoAlunoRepoFactory = acessoAlunoRepoFactory ?? throw new ArgumentNullException(nameof(acessoAlunoRepoFactory));
        _acessoColaboradorRepoFactory = acessoColaboradorRepoFactory ?? throw new ArgumentNullException(nameof(acessoColaboradorRepoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
        _colaboradorRepoFactory = colaboradorRepoFactory ?? throw new ArgumentNullException(nameof(colaboradorRepoFactory));
        _matriculaRepoFactory = matriculaRepoFactory ?? throw new ArgumentNullException(nameof(matriculaRepoFactory));
    }

    public async Task<bool> AlunoEstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        return await _acessoAlunoRepoFactory().EstaNaAcademia(alunoId, cancellationToken);
    }

    public async Task<bool> ColaboradorEstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        return await _acessoColaboradorRepoFactory().EstaNaAcademia(colaboradorId, cancellationToken);
    }

    public async Task<AcessoResultadoDto> RegistrarAcessoAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var aluno = await _alunoRepoFactory().ObterPorId(alunoId, cancellationToken)
            ?? throw new InvalidOperationException($"Aluno com ID {alunoId} não encontrado.");

        bool estaDentro = await _acessoAlunoRepoFactory().EstaNaAcademia(alunoId, cancellationToken);

        if (!estaDentro)
        {
            // Valida se possui matrícula ativa
            var matriculaAtiva = await _matriculaRepoFactory().ObterMatriculaAtivaPorAluno(alunoId, cancellationToken);
            if (matriculaAtiva == null)
            {
                return new AcessoResultadoDto
                {
                    Sucesso = false,
                    TipoResultado = TipoAcessoResultado.AcessoNegado,
                    Titulo = "Acesso Negado",
                    Mensagem = "Acesso negado: o aluno não possui matrícula ativa.",
                    EhEntrada = true
                };
            }

            // Calcula dias restantes de plano
            int diasRestantes = (matriculaAtiva.DataFim.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;
            if (diasRestantes < 0) diasRestantes = 0;

            // Registra entrada
            var acesso = AcessoAluno.Carregar(0, alunoId, DateTime.Now);
            await _acessoAlunoRepoFactory().Adicionar(acesso, cancellationToken);

            return new AcessoResultadoDto
            {
                Sucesso = true,
                TipoResultado = TipoAcessoResultado.EntradaAutorizada,
                Titulo = "Entrada Autorizada",
                Mensagem = $"Entrada autorizada. Dias restantes de plano: {diasRestantes} dia(s).",
                EhEntrada = true,
                DiasRestantesPlano = diasRestantes
            };
        }
        else
        {
            // Registra saída
            var ultimoAcesso = await _acessoAlunoRepoFactory().ObterUltimoAcesso(alunoId, cancellationToken);
            TimeSpan permanencia = ultimoAcesso != null ? DateTime.Now - ultimoAcesso.DataHora : TimeSpan.Zero;
            if (permanencia < TimeSpan.Zero) permanencia = TimeSpan.Zero;

            var acesso = AcessoAluno.Carregar(0, alunoId, DateTime.Now);
            await _acessoAlunoRepoFactory().Adicionar(acesso, cancellationToken);

            return new AcessoResultadoDto
            {
                Sucesso = true,
                TipoResultado = TipoAcessoResultado.SaidaRegistrada,
                Titulo = "Saída Registrada",
                Mensagem = $"Saída registrada. Tempo de permanência: {permanencia:hh\\:mm\\:ss}.",
                EhEntrada = false,
                TempoPermanencia = permanencia
            };
        }
    }

    public async Task<AcessoResultadoDto> RegistrarAcessoColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var colaborador = await _colaboradorRepoFactory().ObterPorId(colaboradorId, cancellationToken)
            ?? throw new InvalidOperationException($"Colaborador com ID {colaboradorId} não encontrado.");

        bool estaDentro = await _acessoColaboradorRepoFactory().EstaNaAcademia(colaboradorId, cancellationToken);

        if (!estaDentro)
        {
            // Valida limite de jornada (8h para CLT, 6h para Estágio)
            var horasHoje = await _acessoColaboradorRepoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, DateOnly.FromDateTime(DateTime.Today), cancellationToken);
            var limite = colaborador.Vinculo == ColaboradorVinculo.Clt ? TimeSpan.FromHours(8) : TimeSpan.FromHours(6);

            if (horasHoje >= limite)
            {
                int limiteHoras = colaborador.Vinculo == ColaboradorVinculo.Clt ? 8 : 6;
                return new AcessoResultadoDto
                {
                    Sucesso = false,
                    TipoResultado = TipoAcessoResultado.AcessoNegado,
                    Titulo = "Acesso Negado",
                    Mensagem = $"Acesso negado: colaborador já atingiu o limite de jornada de {limiteHoras} horas no dia.",
                    EhEntrada = true,
                    HorasTrabalhadasHoje = horasHoje
                };
            }

            // Registra entrada
            var acesso = AcessoColaborador.Carregar(0, colaboradorId, DateTime.Now);
            await _acessoColaboradorRepoFactory().Adicionar(acesso, cancellationToken);

            return new AcessoResultadoDto
            {
                Sucesso = true,
                TipoResultado = TipoAcessoResultado.EntradaAutorizada,
                Titulo = "Entrada Autorizada",
                Mensagem = $"Entrada registrada. Horas trabalhadas hoje até o momento: {horasHoje:hh\\:mm\\:ss}.",
                EhEntrada = true,
                HorasTrabalhadasHoje = horasHoje
            };
        }
        else
        {
            // Registra saída
            var ultimoAcesso = await _acessoColaboradorRepoFactory().ObterUltimoAcesso(colaboradorId, cancellationToken);
            TimeSpan permanencia = ultimoAcesso != null ? DateTime.Now - ultimoAcesso.DataHora : TimeSpan.Zero;
            if (permanencia < TimeSpan.Zero) permanencia = TimeSpan.Zero;

            var acesso = AcessoColaborador.Carregar(0, colaboradorId, DateTime.Now);
            await _acessoColaboradorRepoFactory().Adicionar(acesso, cancellationToken);

            var totalHorasHoje = await _acessoColaboradorRepoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, DateOnly.FromDateTime(DateTime.Today), cancellationToken);

            return new AcessoResultadoDto
            {
                Sucesso = true,
                TipoResultado = TipoAcessoResultado.SaidaRegistrada,
                Titulo = "Saída Registrada",
                Mensagem = $"Saída registrada. Tempo de permanência: {permanencia:hh\\:mm\\:ss}.",
                EhEntrada = false,
                TempoPermanencia = permanencia,
                HorasTrabalhadasHoje = totalHorasHoje
            };
        }
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasHojeColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        return await _acessoColaboradorRepoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, DateOnly.FromDateTime(DateTime.Today), cancellationToken);
    }

    public async Task<IEnumerable<AcessoHistoricoItemDto>> ObterAcessosHojeAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var itens = new List<AcessoHistoricoItemDto>();

        // 1. Acessos de Alunos hoje
        var acessosAlunos = (await _acessoAlunoRepoFactory().ObterAcessosPorAlunoPeriodo(null, hoje, hoje, cancellationToken)).ToList();
        var gruposAlunos = acessosAlunos.GroupBy(a => a.AlunoId);

        foreach (var grupo in gruposAlunos)
        {
            var aluno = await _alunoRepoFactory().ObterPorId(grupo.Key, cancellationToken);
            if (aluno == null) continue;

            var acessosOrdenados = grupo.OrderBy(a => a.DataHora).ToList();
            for (int i = 0; i < acessosOrdenados.Count; i++)
            {
                var acesso = acessosOrdenados[i];
                bool ehEntrada = (i % 2 == 0);
                string descricao;

                if (ehEntrada)
                {
                    var matricula = await _matriculaRepoFactory().ObterMatriculaAtivaPorAluno(aluno.Id, cancellationToken);
                    int dias = matricula != null ? Math.Max(0, (matricula.DataFim.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days) : 0;
                    descricao = $"Entrada autorizada. Dias restantes de plano: {dias} dia(s).";
                }
                else
                {
                    var entradaAnterior = acessosOrdenados[i - 1];
                    var duracao = acesso.DataHora - entradaAnterior.DataHora;
                    if (duracao < TimeSpan.Zero) duracao = TimeSpan.Zero;
                    descricao = $"Saída registrada. Tempo de permanência: {duracao:hh\\:mm\\:ss}.";
                }

                itens.Add(new AcessoHistoricoItemDto
                {
                    Id = acesso.Id,
                    PessoaId = aluno.Id,
                    NomePessoa = aluno.Nome,
                    CpfPessoa = aluno.Cpf.Valor,
                    FotoPessoa = aluno.Foto?.Conteudo,
                    EhAluno = true,
                    EhEntrada = ehEntrada,
                    Descricao = descricao,
                    DataHora = acesso.DataHora
                });
            }
        }

        // 2. Acessos de Colaboradores hoje
        var acessosColaboradores = (await _acessoColaboradorRepoFactory().ObterAcessosPorColaboradorPeriodo(null, hoje, hoje, cancellationToken)).ToList();
        var gruposColab = acessosColaboradores.GroupBy(a => a.ColaboradorId);

        foreach (var grupo in gruposColab)
        {
            var colab = await _colaboradorRepoFactory().ObterPorId(grupo.Key, cancellationToken);
            if (colab == null) continue;

            var acessosOrdenados = grupo.OrderBy(a => a.DataHora).ToList();
            TimeSpan horasAcumuladas = TimeSpan.Zero;

            for (int i = 0; i < acessosOrdenados.Count; i++)
            {
                var acesso = acessosOrdenados[i];
                bool ehEntrada = (i % 2 == 0);
                string descricao;

                if (ehEntrada)
                {
                    descricao = $"Entrada registrada. Horas trabalhadas hoje até o momento: {horasAcumuladas:hh\\:mm\\:ss}.";
                }
                else
                {
                    var entradaAnterior = acessosOrdenados[i - 1];
                    var duracao = acesso.DataHora - entradaAnterior.DataHora;
                    if (duracao < TimeSpan.Zero) duracao = TimeSpan.Zero;
                    horasAcumuladas += duracao;
                    descricao = $"Saída registrada. Tempo de permanência: {duracao:hh\\:mm\\:ss}.";
                }

                itens.Add(new AcessoHistoricoItemDto
                {
                    Id = acesso.Id,
                    PessoaId = colab.Id,
                    NomePessoa = colab.Nome,
                    CpfPessoa = colab.Cpf.Valor,
                    FotoPessoa = colab.Foto?.Conteudo,
                    EhAluno = false,
                    EhEntrada = ehEntrada,
                    Descricao = descricao,
                    DataHora = acesso.DataHora
                });
            }
        }

        // Ordena histórico geral pelo mais recente primeiro
        return itens.OrderByDescending(x => x.DataHora);
    }
}
