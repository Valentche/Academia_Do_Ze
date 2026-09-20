//Pablo Valente Neto

/*
 * Testes de integração do LogradouroRepository. Cobrem todos os contratos definidos em
 * ILogradouroRepository (IRepository<Logradouro> + métodos específicos do domínio):
 *
 * - Task<Logradouro?> ObterPorId(int id, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Logradouro>> ObterTodos(CancellationToken cancellationToken = default)
 * - Task<Logradouro> Adicionar(Logradouro entity, CancellationToken cancellationToken = default)
 * - Task<Logradouro> Atualizar(Logradouro entity, CancellationToken cancellationToken = default)
 * - Task<bool> Remover(int id, CancellationToken cancellationToken = default)
 * - Task<Logradouro?> ObterPorCep(Cep cep, CancellationToken cancellationToken = default)
 * - Task<bool> CepJaExiste(Cep cep, int? id = null, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Logradouro>> ObterPorBairro(string cidade, string bairro, CancellationToken cancellationToken = default)
 *
 * Como as regras de negócio já foram validadas na camada de domínio, aqui o foco é
 * exclusivamente nas operações de banco de dados, por isso os CEPs são randomizados.
 */

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    // Id que não existe na base, usado nos cenários negativos.
    private const int IdInexistente = 999999;

    private readonly LogradouroRepository _repository;

    public LogradouroInfrastructureTests()
    {
        _repository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    // Gera e insere um Logradouro padrão com CEP único para reuso nas asserções dos testes.
    // Também é usado pelos testes das entidades que dependem de Logradouro (Colaborador, Aluno).
    internal static async Task<Logradouro> CriarEInserirLogradouroAsync(LogradouroRepository logradouroRepo)
    {
        var logradouroResult = Logradouro.Criar(0, GerarCep(), NomeRua, NomeBairro, NomeCidade, NomeEstado, NomePais);

        if (logradouroResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Logradouro: {string.Join(", ", logradouroResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await logradouroRepo.Adicionar(logradouroResult.Value!);
    }

    [Fact(DisplayName = "Logradouro: Adicionar e ObterPorId com sucesso")]
    public async Task Logradouro_Adicionar_E_ObterPorId_Sucesso()
    {
        var cep = GerarCep();
        var logradouro = Logradouro.Criar(0, cep, NomeRua, NomeBairro, NomeCidade, NomeEstado, NomePais).Value!;

        var inserido = await _repository.Adicionar(logradouro);

        Assert.NotNull(inserido);
        Assert.True(inserido.Id > 0);
        Assert.Equal(cep, inserido.Cep.Valor);
        Assert.Equal(NomeRua, inserido.Nome);

        var obtido = await _repository.ObterPorId(inserido.Id);

        Assert.NotNull(obtido);
        Assert.Equal(inserido.Id, obtido.Id);
        Assert.Equal(cep, obtido.Cep.Valor);
        Assert.Equal(NomeRua, obtido.Nome);
        Assert.Equal(NomeBairro, obtido.Bairro);
        Assert.Equal(NomeCidade, obtido.Cidade);
        Assert.Equal(NomeEstado, obtido.Estado);
        Assert.Equal(NomePais, obtido.Pais);
    }

    [Fact(DisplayName = "Logradouro: ObterPorId retorna nulo quando o registro não existe")]
    public async Task Logradouro_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(IdInexistente);

        Assert.Null(obtido);
    }

    [Fact(DisplayName = "Logradouro: ObterTodos retorna a lista preenchida")]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var todos = await _repository.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
        Assert.Contains(todos, l => l.Id == logradouro.Id);
    }

    [Fact(DisplayName = "Logradouro: Atualizar persiste as alterações")]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        // Inserido com dados provisórios para que o UPDATE possa ser efetivamente comprovado.
        var original = Logradouro.Criar(0, GerarCep(), "Rua Provisoria", "Bairro Provisorio", "Cidade Provisoria", "SP", NomePais).Value!;
        var inserido = await _repository.Adicionar(original);

        var novoCep = GerarCep();
        var logradouroAtualizado = Logradouro.Criar(inserido.Id, novoCep, NomeRua, NomeBairro, NomeCidade, NomeEstado, NomePais).Value!;

        var resultado = await _repository.Atualizar(logradouroAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal(NomeRua, resultado.Nome);
        Assert.Equal(NomeBairro, resultado.Bairro);
        Assert.Equal(NomeCidade, resultado.Cidade);

        var noBanco = await _repository.ObterPorId(inserido.Id);

        Assert.NotNull(noBanco);
        Assert.Equal(novoCep, noBanco.Cep.Valor);
        Assert.Equal(NomeRua, noBanco.Nome);
        Assert.Equal(NomeBairro, noBanco.Bairro);
        Assert.Equal(NomeCidade, noBanco.Cidade);
    }

    [Fact(DisplayName = "Logradouro: Atualizar lança REGISTRO_NAO_ENCONTRADO quando o registro não existe")]
    public async Task Logradouro_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouroInexistente = Logradouro.Criar(IdInexistente, GerarCep(), NomeRua, NomeBairro, NomeCidade, NomeEstado, NomePais).Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _repository.Atualizar(logradouroInexistente));

        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact(DisplayName = "Logradouro: Remover apaga o registro do banco")]
    public async Task Logradouro_Remover_Sucesso()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var removido = await _repository.Remover(logradouro.Id);

        Assert.True(removido);

        var noBanco = await _repository.ObterPorId(logradouro.Id);

        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Logradouro: Remover retorna false quando o registro não existe")]
    public async Task Logradouro_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _repository.Remover(IdInexistente);

        Assert.False(removido);
    }

    [Fact(DisplayName = "Logradouro: ObterPorCep encontra o registro e retorna nulo para CEP inexistente")]
    public async Task Logradouro_ObterPorCep_SucessoENulo()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var obtido = await _repository.ObterPorCep(logradouro.Cep);

        Assert.NotNull(obtido);
        Assert.Equal(logradouro.Id, obtido.Id);
        Assert.Equal(logradouro.Cep.Valor, obtido.Cep.Valor);

        var cepInexistente = Cep.Criar("99999999").Value!;
        var naoObtido = await _repository.ObterPorCep(cepInexistente);

        Assert.Null(naoObtido);
    }

    [Fact(DisplayName = "Logradouro: CepJaExiste valida duplicidade ignorando o próprio Id")]
    public async Task Logradouro_CepJaExiste_ValidacaoCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        // O CEP existe na base.
        var existe = await _repository.CepJaExiste(logradouro.Cep);
        Assert.True(existe);

        // Ao editar o próprio registro, o CEP dele não deve ser considerado duplicado.
        var existeMesmoId = await _repository.CepJaExiste(logradouro.Cep, logradouro.Id);
        Assert.False(existeMesmoId);

        // Um CEP nunca inserido não pode ser encontrado.
        var cepInedito = Cep.Criar(GerarCep()).Value!;
        var existeInedito = await _repository.CepJaExiste(cepInedito);
        Assert.False(existeInedito);
    }

    [Fact(DisplayName = "Logradouro: ObterPorCidade filtra corretamente pela cidade")]
    public async Task Logradouro_ObterPorCidade_FiltragemCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var resultados = await _repository.ObterPorCidade(NomeCidade);

        Assert.NotNull(resultados);
        Assert.NotEmpty(resultados);
        Assert.Contains(resultados, l => l.Id == logradouro.Id);
        Assert.All(resultados, l => Assert.Equal(NomeCidade, l.Cidade));

        var resultadosVazio = await _repository.ObterPorCidade("CidadeInexistente_123");

        Assert.Empty(resultadosVazio);
    }

    [Fact(DisplayName = "Logradouro: ObterPorBairro filtra corretamente por cidade e bairro")]
    public async Task Logradouro_ObterPorBairro_FiltragemCorreta()
    {
        var logradouro = await CriarEInserirLogradouroAsync(_repository);

        var resultados = await _repository.ObterPorBairro(NomeCidade, NomeBairro);

        Assert.NotNull(resultados);
        Assert.NotEmpty(resultados);
        Assert.Contains(resultados, l => l.Id == logradouro.Id);
        Assert.All(resultados, l =>
        {
            Assert.Equal(NomeCidade, l.Cidade);
            Assert.Equal(NomeBairro, l.Bairro);
        });

        var resultadosVazio = await _repository.ObterPorBairro(NomeCidade, "BairroInexistente_123");

        Assert.Empty(resultadosVazio);
    }

    [Fact(DisplayName = "Infra: string de conexão vazia lança CONEXAO_STRING_VAZIA")]
    public async Task Logradouro_LancaExcecaoQuandoStringConexaoVazia()
    {
        await using var repositorio = new LogradouroRepository(string.Empty, DatabaseType);

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => repositorio.ObterTodos());

        Assert.Equal("CONEXAO_STRING_VAZIA", ex.ErrorCode);
    }

    [Fact(DisplayName = "Infra: credenciais/caminho inválidos lançam InfrastructureException")]
    public async Task Logradouro_LancaExcecaoQuandoCredenciaisInvalidas()
    {
        // Credenciais (ou caminho, no caso do SQLite) propositalmente inválidas.
        var conexaoInvalida = DatabaseType switch
        {
            DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=usuario_invalido;Password=senha_invalida;TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;",
            DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=usuario_invalido;Password=senha_invalida;Connection Timeout=5;",
            DatabaseType.Sqlite => @"Data Source=Z:\diretorio_inexistente\db_academia_do_ze.db;",
            _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
        };

        await using var repositorio = new LogradouroRepository(conexaoInvalida, DatabaseType);

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => repositorio.ObterTodos());

        Assert.False(string.IsNullOrWhiteSpace(ex.ErrorCode));
    }

    [Fact(DisplayName = "Infra: SGBD não suportado lança SGDB_NAO_SUPORTADO")]
    public void DbProvider_LancaExcecaoQuandoSgbdNaoSuportado()
    {
        var sgbdInvalido = (DatabaseType)999;

        var ex = Assert.Throws<InfrastructureException>(() => DbProvider.CreateConnection("Data Source=teste;", sgbdInvalido));

        Assert.Equal("SGDB_NAO_SUPORTADO", ex.ErrorCode);
    }
}
