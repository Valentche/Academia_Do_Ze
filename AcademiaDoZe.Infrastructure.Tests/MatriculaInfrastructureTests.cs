// Pablo Valente Neto

/*
 * Testes de integração do MatriculaRepository. Cobrem todos os contratos de IMatriculaRepository:
 *
 * - Task<Matricula?> ObterPorId(int id, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterTodos(CancellationToken cancellationToken = default)
 * - Task<Matricula> Adicionar(Matricula entity, CancellationToken cancellationToken = default)
 * - Task<Matricula> Atualizar(Matricula entity, CancellationToken cancellationToken = default)
 * - Task<bool> Remover(int id, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId, CancellationToken cancellationToken = default)
 * - Task<Matricula?> ObterMatriculaAtivaPorAluno(int alunoId, CancellationToken cancellationToken = default)
 * - Task<bool> PossuiMatriculaAtiva(int alunoId, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterAtivas(int alunoId = 0, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Matricula>> ObterPorPlano(MatriculaPlano plano, CancellationToken cancellationToken = default)
 *
 * Requisitos da atividade 07: objetivo = NomePessoa e obs_restricao contendo a sigla do SGBD em uso.
 */

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class MatriculaInfrastructureTests : TestBase
{
    // Id que não existe na base, usado nos cenários negativos.
    private const int IdInexistente = 999999;

    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;
    private readonly MatriculaRepository _matriculaRepo;

    public MatriculaInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
        _matriculaRepo = new MatriculaRepository(ConnectionString, DatabaseType);
    }

    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.Today);

    // Requisito da atividade 07: toda obs_restricao gravada precisa conter a sigla do SGBD em uso.
    private static string ObsComSgbd(string texto) => $"{texto} - {NomeCidade}";

    // Gera e insere uma Matricula para o aluno informado, já atendendo aos requisitos da atividade.
    private async Task<Matricula> CriarEInserirMatriculaAsync(
        Aluno aluno,
        MatriculaPlano plano = MatriculaPlano.Mensal,
        DateOnly? dataInicio = null,
        MatriculaRestricoes restricoes = MatriculaRestricoes.None,
        string? obsRestricao = null,
        Arquivo? laudo = null)
    {
        var inicio = dataInicio ?? Hoje;

        // O domínio exige laudo sempre que houver restrição médica.
        if (restricoes != MatriculaRestricoes.None && laudo == null)
        {
            laudo = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value;
        }

        var matriculaResult = Matricula.Criar(
            id: 0,
            aluno: aluno,
            plano: plano,
            dataInicio: inicio,
            objetivo: NomePessoa,
            restricoesMedicas: restricoes,
            laudoMedico: laudo,
            observacoesRestricoes: obsRestricao ?? ObsComSgbd("Observação de teste"));

        if (matriculaResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Matricula no Helper: {string.Join(", ", matriculaResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await _matriculaRepo.Adicionar(matriculaResult.Value!);
    }

    [Fact(DisplayName = "Matrícula: Adicionar e ObterPorId com restrições combinadas e laudo")]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var restricoesComb = MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta;
        var laudo = Arquivo.Criar(new byte[] { 100, 101, 102 }).Value;
        var obs = ObsComSgbd("Usar medicação ao acordar");

        var inserida = await CriarEInserirMatriculaAsync(aluno: aluno, plano: MatriculaPlano.Mensal, restricoes: restricoesComb, obsRestricao: obs, laudo: laudo);

        Assert.NotNull(inserida);
        Assert.True(inserida.Id > 0);
        Assert.Equal(aluno.Id, inserida.AlunoId);
        Assert.Equal(MatriculaPlano.Mensal, inserida.Plano);
        Assert.Equal(restricoesComb, inserida.RestricoesMedicas);
        Assert.True(inserida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(inserida.RestricoesMedicas.HasFlag(MatriculaRestricoes.PressaoAlta));

        var obtida = await _matriculaRepo.ObterPorId(inserida.Id);

        Assert.NotNull(obtida);
        Assert.Equal(inserida.Id, obtida.Id);
        Assert.Equal(aluno.Id, obtida.AlunoId);
        Assert.Equal(MatriculaPlano.Mensal, obtida.Plano);
        Assert.Equal(inserida.DataInicio, obtida.DataInicio);
        Assert.Equal(NomePessoa, obtida.Objetivo);
        Assert.Equal(restricoesComb, obtida.RestricoesMedicas);
        Assert.Equal(obs, obtida.ObservacoesRestricoes);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(laudo!.Conteudo, obtida.LaudoMedico.Conteudo);
    }

    [Fact(DisplayName = "Matrícula: restrições de múltipla escolha persistem e voltam corretamente")]
    public async Task Matricula_RestricoesMedicas_ComVariacoesMultiplaEscolha_PersisteEObtemCorretamente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var laudo = Arquivo.Criar(new byte[] { 10, 20, 30, 40, 50 }).Value!;
        var obs = ObsComSgbd("Evitar exercícios de alto impacto e hipertensão");

        // Combinação de múltipla escolha: Pressão Alta + Labirintite + Problemas Respiratórios + Remédio Contínuo
        var restricoesMultiplas = MatriculaRestricoes.PressaoAlta | MatriculaRestricoes.Labirintite | MatriculaRestricoes.ProblemasRespiratorios | MatriculaRestricoes.RemedioContinuo;

        var matricula = await CriarEInserirMatriculaAsync(aluno: aluno, plano: MatriculaPlano.Semestral, restricoes: restricoesMultiplas, obsRestricao: obs, laudo: laudo);

        var obtida = await _matriculaRepo.ObterPorId(matricula.Id);

        Assert.NotNull(obtida);
        Assert.Equal(restricoesMultiplas, obtida.RestricoesMedicas);

        // Verificação bitwise das flags selecionadas
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.PressaoAlta));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.ProblemasRespiratorios));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.RemedioContinuo));

        // Flags não marcadas precisam voltar como false
        Assert.False(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.False(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Alergias));

        Assert.Equal(obs, obtida.ObservacoesRestricoes);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(laudo.Conteudo, obtida.LaudoMedico.Conteudo);
    }

    [Fact(DisplayName = "Matrícula: ObterPorId retorna nulo quando o registro não existe")]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtida = await _matriculaRepo.ObterPorId(IdInexistente);

        Assert.Null(obtida);
    }

    [Fact(DisplayName = "Matrícula: ObterTodos retorna a lista preenchida")]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno);

        var todas = await _matriculaRepo.ObterTodos();

        Assert.NotNull(todas);
        Assert.NotEmpty(todas);
        Assert.Contains(todas, m => m.Id == inserida.Id);
    }

    [Fact(DisplayName = "Matrícula: Atualizar lança REGISTRO_NAO_ENCONTRADO quando o registro não existe")]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var matriculaInexistente = Matricula.Criar(
            id: IdInexistente,
            aluno: aluno,
            plano: MatriculaPlano.Mensal,
            dataInicio: Hoje,
            objetivo: NomePessoa,
            restricoesMedicas: MatriculaRestricoes.None,
            laudoMedico: null,
            observacoesRestricoes: ObsComSgbd("Matrícula inexistente")).Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _matriculaRepo.Atualizar(matriculaInexistente));

        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact(DisplayName = "Matrícula: Atualizar persiste plano, objetivo, restrições, laudo e observação")]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        // Inserida com objetivo e observação provisórios para que o UPDATE possa ser efetivamente comprovado.
        var original = Matricula.Criar(
            id: 0,
            aluno: aluno,
            plano: MatriculaPlano.Mensal,
            dataInicio: Hoje,
            objetivo: "Objetivo Provisorio",
            restricoesMedicas: MatriculaRestricoes.Alergias,
            laudoMedico: Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value,
            observacoesRestricoes: "Observação provisória").Value!;
        var inserida = await _matriculaRepo.Adicionar(original);

        var novasRestricoes = MatriculaRestricoes.Alergias | MatriculaRestricoes.Diabetes | MatriculaRestricoes.Labirintite;
        var laudoAtualizado = Arquivo.Criar(new byte[] { 99, 88, 77 }).Value!;
        var novaObs = ObsComSgbd("Restrição médica atualizada com novas opções");

        var matriculaAtualizada = Matricula.Criar(
            id: inserida.Id,
            aluno: aluno,
            plano: MatriculaPlano.Anual,
            dataInicio: inserida.DataInicio,
            objetivo: NomePessoa,
            restricoesMedicas: novasRestricoes,
            laudoMedico: laudoAtualizado,
            observacoesRestricoes: novaObs).Value!;

        var resultado = await _matriculaRepo.Atualizar(matriculaAtualizada);

        Assert.NotNull(resultado);
        Assert.Equal(MatriculaPlano.Anual, resultado.Plano);
        Assert.Equal(NomePessoa, resultado.Objetivo);
        Assert.Equal(novasRestricoes, resultado.RestricoesMedicas);

        var noBanco = await _matriculaRepo.ObterPorId(inserida.Id);

        Assert.NotNull(noBanco);
        Assert.Equal(MatriculaPlano.Anual, noBanco.Plano);
        Assert.Equal(NomePessoa, noBanco.Objetivo);
        Assert.Equal(novaObs, noBanco.ObservacoesRestricoes);
        Assert.Equal(novasRestricoes, noBanco.RestricoesMedicas);
        Assert.True(noBanco.RestricoesMedicas.HasFlag(MatriculaRestricoes.Alergias));
        Assert.True(noBanco.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.True(noBanco.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.NotNull(noBanco.LaudoMedico);
        Assert.Equal(laudoAtualizado.Conteudo, noBanco.LaudoMedico.Conteudo);
    }

    [Fact(DisplayName = "Matrícula: Remover apaga o registro do banco")]
    public async Task Matricula_Remover_Sucesso()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno);

        var removida = await _matriculaRepo.Remover(inserida.Id);

        Assert.True(removida);

        var noBanco = await _matriculaRepo.ObterPorId(inserida.Id);

        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Matrícula: Remover retorna false quando o registro não existe")]
    public async Task Matricula_Remover_RetornaFalseQuandoInexistente()
    {
        var removida = await _matriculaRepo.Remover(IdInexistente);

        Assert.False(removida);
    }

    [Fact(DisplayName = "Matrícula: ObterPorAluno filtra corretamente pelo aluno")]
    public async Task Matricula_ObterPorAluno_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno);

        var matriculas = await _matriculaRepo.ObterPorAluno(aluno.Id);

        Assert.NotNull(matriculas);
        Assert.NotEmpty(matriculas);
        Assert.Contains(matriculas, m => m.Id == inserida.Id);
        Assert.All(matriculas, m => Assert.Equal(aluno.Id, m.AlunoId));
    }

    [Fact(DisplayName = "Matrícula: vencida não conta como ativa; a vigente sim")]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_E_PossuiMatriculaAtiva()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        Assert.False(await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id));

        // Mensal iniciada há 2 meses: terminou há cerca de 1 mês, não pode ser considerada ativa.
        await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, Hoje.AddMonths(-2));

        Assert.False(await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id));
        Assert.Null(await _matriculaRepo.ObterMatriculaAtivaPorAluno(aluno.Id));

        var vigente = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, Hoje);

        Assert.True(await _matriculaRepo.PossuiMatriculaAtiva(aluno.Id));

        var ativa = await _matriculaRepo.ObterMatriculaAtivaPorAluno(aluno.Id);

        Assert.NotNull(ativa);
        Assert.Equal(vigente.Id, ativa.Id);
        Assert.Equal(aluno.Id, ativa.AlunoId);
    }

    [Fact(DisplayName = "Matrícula: ObterAtivas ignora vencidas e filtra por aluno")]
    public async Task Matricula_ObterAtivas_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var vencida = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, Hoje.AddMonths(-2));
        var vigente = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Semestral, Hoje);

        var ativasGeral = await _matriculaRepo.ObterAtivas();

        Assert.NotNull(ativasGeral);
        Assert.Contains(ativasGeral, m => m.Id == vigente.Id);
        Assert.DoesNotContain(ativasGeral, m => m.Id == vencida.Id);

        var ativasPorAluno = await _matriculaRepo.ObterAtivas(aluno.Id);

        Assert.NotNull(ativasPorAluno);
        var unica = Assert.Single(ativasPorAluno);
        Assert.Equal(vigente.Id, unica.Id);
    }

    [Fact(DisplayName = "Matrícula: ObterVencendoEmDias traz só as que vencem dentro da janela")]
    public async Task Matricula_ObterVencendoEmDias_RetornaMatriculasProximasDoVencimento()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        // Mensal iniciada há 25 dias: vence em poucos dias, dentro da janela de 30.
        var proxima = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Mensal, Hoje.AddDays(-25));

        // Anual iniciada hoje: vence daqui a 12 meses, fora da janela.
        var distante = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Anual, Hoje);

        var vencendoEm30Dias = await _matriculaRepo.ObterVencendoEmDias(30);

        Assert.NotNull(vencendoEm30Dias);
        Assert.Contains(vencendoEm30Dias, m => m.Id == proxima.Id);
        Assert.DoesNotContain(vencendoEm30Dias, m => m.Id == distante.Id);
    }

    [Fact(DisplayName = "Matrícula: ObterPorPlano filtra corretamente pelo plano")]
    public async Task Matricula_ObterPorPlano_FiltragemCorreta()
    {
        var aluno = await AlunoInfrastructureTests.CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);
        var inserida = await CriarEInserirMatriculaAsync(aluno, MatriculaPlano.Trimestral);

        var trimestrais = await _matriculaRepo.ObterPorPlano(MatriculaPlano.Trimestral);

        Assert.NotNull(trimestrais);
        Assert.Contains(trimestrais, m => m.Id == inserida.Id);
        Assert.All(trimestrais, m => Assert.Equal(MatriculaPlano.Trimestral, m.Plano));
    }
}
