// Pablo Valente Neto

/*
 * Testes de integração do AlunoRepository. Cobrem todos os contratos de IAlunoRepository:
 *
 * - Task<Aluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Aluno>> ObterTodos(CancellationToken cancellationToken = default)
 * - Task<Aluno> Adicionar(Aluno entity, CancellationToken cancellationToken = default)
 * - Task<Aluno> Atualizar(Aluno entity, CancellationToken cancellationToken = default)
 * - Task<bool> Remover(int id, CancellationToken cancellationToken = default)
 * - Task<Aluno?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default)
 * - Task<Aluno?> ObterPorEmail(Email email, CancellationToken cancellationToken = default)
 * - Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default)
 * - Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default)
 * - Task<IEnumerable<Aluno>> ObterPorNome(string nome, CancellationToken cancellationToken = default)
 * - Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default)
 *
 * Requisitos da atividade: nome = NomePessoa, complemento = NomeComplemento e senha
 * contendo a sigla do SGBD em uso (SenhaTeste).
 */

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    // Id que não existe na base, usado nos cenários negativos.
    private const int IdInexistente = 999999;

    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _alunoRepo;

    public AlunoInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
    }

    // Gera e insere um Aluno padrão, com o Logradouro que ele referencia, para reuso nos testes.
    internal static async Task<Aluno> CriarEInserirAlunoAsync(AlunoRepository alunoRepo, LogradouroRepository logradouroRepo)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value!;

        var alunoResult = Aluno.Criar(
            id: 0,
            nome: NomePessoa,
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(2000, 3, 20),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: logradouro,
            numero: "100",
            complemento: NomeComplemento,
            senha: SenhaTeste,
            foto: foto);

        if (alunoResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Aluno: {string.Join(", ", alunoResult.Notifications.Select(n => n.Mensagem))}");
        }

        return await alunoRepo.Adicionar(alunoResult.Value!);
    }

    [Fact(DisplayName = "Aluno: Adicionar e ObterPorId com sucesso")]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        Assert.NotNull(aluno);
        Assert.True(aluno.Id > 0);

        var obtido = await _alunoRepo.ObterPorId(aluno.Id);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Equal(aluno.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal(NomePessoa, obtido.Nome);
        Assert.Equal(aluno.Email.Valor, obtido.Email.Valor);
        Assert.Equal(NomeComplemento, obtido.Endereco.Complemento);
        Assert.Equal(SenhaTeste, obtido.Senha.Valor);
        Assert.NotNull(obtido.Endereco);
        Assert.Equal(aluno.Endereco.LogradouroId, obtido.Endereco.LogradouroId);
    }

    [Fact(DisplayName = "Aluno: ObterPorId retorna nulo quando o registro não existe")]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _alunoRepo.ObterPorId(IdInexistente);

        Assert.Null(obtido);
    }

    [Fact(DisplayName = "Aluno: ObterTodos retorna a lista preenchida")]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var todos = await _alunoRepo.ObterTodos();

        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
        Assert.Contains(todos, a => a.Id == aluno.Id);
    }

    [Fact(DisplayName = "Aluno: Atualizar persiste as alterações")]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value!;

        // Inserido com dados provisórios para que o UPDATE possa ser efetivamente comprovado.
        var aluno = await _alunoRepo.Adicionar(Aluno.Criar(
            0, NomePessoa, GerarCpf(), new DateOnly(2000, 3, 20), GerarTelefone(), GerarEmail(),
            logradouro, "100", "Complemento Provisorio", "SenhaProvisoria123", foto).Value!);

        var novoTelefone = GerarTelefone();
        var alunoAtualizado = Aluno.Criar(
            id: aluno.Id,
            nome: NomePessoa,
            cpf: aluno.Cpf.Valor,
            dataNascimento: aluno.DataNascimento,
            telefone: novoTelefone,
            email: aluno.Email.Valor,
            endereco: logradouro,
            numero: "200",
            complemento: NomeComplemento,
            senha: SenhaTeste,
            foto: aluno.Foto).Value!;

        var resultado = await _alunoRepo.Atualizar(alunoAtualizado);

        Assert.NotNull(resultado);
        Assert.Equal(NomeComplemento, resultado.Endereco.Complemento);

        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);

        Assert.NotNull(noBanco);
        Assert.Equal(novoTelefone, noBanco.Telefone.Valor);
        Assert.Equal("200", noBanco.Endereco.Numero);
        Assert.Equal(NomeComplemento, noBanco.Endereco.Complemento);
        Assert.Equal(SenhaTeste, noBanco.Senha.Valor);
    }

    [Fact(DisplayName = "Aluno: Atualizar lança REGISTRO_NAO_ENCONTRADO quando o registro não existe")]
    public async Task Aluno_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 1, 2 }).Value!;

        var alunoInexistente = Aluno.Criar(
            id: IdInexistente,
            nome: NomePessoa,
            cpf: GerarCpf(),
            dataNascimento: new DateOnly(2000, 3, 20),
            telefone: GerarTelefone(),
            email: GerarEmail(),
            endereco: logradouro,
            numero: "1",
            complemento: NomeComplemento,
            senha: SenhaTeste,
            foto: foto).Value!;

        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _alunoRepo.Atualizar(alunoInexistente));

        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact(DisplayName = "Aluno: Remover apaga o registro do banco")]
    public async Task Aluno_Remover_Sucesso()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var removido = await _alunoRepo.Remover(aluno.Id);

        Assert.True(removido);

        var noBanco = await _alunoRepo.ObterPorId(aluno.Id);

        Assert.Null(noBanco);
    }

    [Fact(DisplayName = "Aluno: Remover retorna false quando o registro não existe")]
    public async Task Aluno_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _alunoRepo.Remover(IdInexistente);

        Assert.False(removido);
    }

    [Fact(DisplayName = "Aluno: ObterPorCpf encontra o registro e retorna nulo para CPF inexistente")]
    public async Task Aluno_ObterPorCpf_SucessoENulo()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var obtido = await _alunoRepo.ObterPorCpf(aluno.Cpf);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);

        var cpfInexistente = Cpf.Criar(GerarCpf()).Value!;
        var naoObtido = await _alunoRepo.ObterPorCpf(cpfInexistente);

        Assert.Null(naoObtido);
    }

    [Fact(DisplayName = "Aluno: ObterPorEmail encontra o registro e retorna nulo para e-mail inexistente")]
    public async Task Aluno_ObterPorEmail_SucessoENulo()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var obtido = await _alunoRepo.ObterPorEmail(aluno.Email);

        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);

        var emailInexistente = Email.Criar(GerarEmail()).Value!;
        var naoObtido = await _alunoRepo.ObterPorEmail(emailInexistente);

        Assert.Null(naoObtido);
    }

    [Fact(DisplayName = "Aluno: CpfJaExiste valida duplicidade ignorando o próprio Id")]
    public async Task Aluno_CpfJaExiste_ValidacaoCorreta()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var existe = await _alunoRepo.CpfJaExiste(aluno.Cpf);
        Assert.True(existe);

        var existeIgnorandoId = await _alunoRepo.CpfJaExiste(aluno.Cpf, aluno.Id);
        Assert.False(existeIgnorandoId);

        var cpfInedito = Cpf.Criar(GerarCpf()).Value!;
        var existeInedito = await _alunoRepo.CpfJaExiste(cpfInedito);
        Assert.False(existeInedito);
    }

    [Fact(DisplayName = "Aluno: EmailJaExiste valida duplicidade ignorando o próprio Id")]
    public async Task Aluno_EmailJaExiste_ValidacaoCorreta()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var existe = await _alunoRepo.EmailJaExiste(aluno.Email);
        Assert.True(existe);

        var existeIgnorandoId = await _alunoRepo.EmailJaExiste(aluno.Email, aluno.Id);
        Assert.False(existeIgnorandoId);

        var emailInedito = Email.Criar(GerarEmail()).Value!;
        var existeInedito = await _alunoRepo.EmailJaExiste(emailInedito);
        Assert.False(existeInedito);
    }

    [Fact(DisplayName = "Aluno: ObterPorNome filtra corretamente pelo nome")]
    public async Task Aluno_ObterPorNome_FiltragemCorreta()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        var resultados = await _alunoRepo.ObterPorNome(NomePessoa);

        Assert.NotNull(resultados);
        Assert.NotEmpty(resultados);
        Assert.Contains(resultados, a => a.Id == aluno.Id);
        Assert.All(resultados, a => Assert.Contains(NomePessoa, a.Nome));

        var resultadosVazio = await _alunoRepo.ObterPorNome("NomeInexistente_123");

        Assert.Empty(resultadosVazio);
    }

    [Fact(DisplayName = "Aluno: TrocarSenha altera a senha e falha para Id inexistente")]
    public async Task Aluno_TrocarSenha_SucessoEFalha()
    {
        var aluno = await CriarEInserirAlunoAsync(_alunoRepo, _logradouroRepo);

        // A nova senha também precisa carregar a sigla do SGBD, exigência da atividade.
        var senhaNova = $"Nova{SenhaTeste}";
        var novaSenha = Senha.Criar(senhaNova).Value!;

        var alterou = await _alunoRepo.TrocarSenha(aluno.Id, novaSenha);
        Assert.True(alterou);

        var atualizado = await _alunoRepo.ObterPorId(aluno.Id);
        Assert.NotNull(atualizado);
        Assert.Equal(senhaNova, atualizado.Senha.Valor);

        var alterouInexistente = await _alunoRepo.TrocarSenha(IdInexistente, novaSenha);
        Assert.False(alterouInexistente);
    }
}
