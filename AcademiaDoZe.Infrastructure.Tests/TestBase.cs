//Pablo Valente Neto

using AcademiaDoZe.Infrastructure.Data;

// Desabilita o paralelismo, garantindo que os testes sejam executados em sequência por assembly,
// evitando condições de corrida (race conditions) ou conflitos de dados no banco durante a suíte.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

// Classe base abstrata que centraliza as configurações comuns aos testes de integração.
// Para alternar o SGBD alvo basta trocar a constante SelectedDatabaseType, o que reflete
// em todas as classes de teste derivadas.
public abstract class TestBase
{
    // Alterne o SGBD alvo dos testes trocando apenas a constante abaixo:
    private const DatabaseType SelectedDatabaseType = DatabaseType.MySql;

    // Caminho do arquivo físico usado pelo SQLite (da pra ver no DB Browser for SQLite).
    private const string SqliteDbPath = @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db";

    protected string ConnectionString { get; }

    protected DatabaseType DatabaseType { get; }

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        // Ajuste a ConnectionString com caminhos e credenciais válidas
        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;",
            DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=root;Password=abcBolinhas12345;",
            DatabaseType.Sqlite => $"Data Source={SqliteDbPath};Cache=Shared;",
            _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
        };

        // O SQLite cria o arquivo do banco automaticamente, mas não o diretório que o contém.
        if (DatabaseType == DatabaseType.Sqlite)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SqliteDbPath)!);
        }
    }

    #region Dados fixos exigidos pelo laboratório

    // Requisito da atividade: rua = nome, bairro = sobrenome, cidade = nome do SGBD em uso.
    protected const string NomeRua = "Pablo";

    protected const string NomeBairro = "Valente Neto";

    // Static para poder ser usada pelos auxiliares static das classes de teste
    // (ex.: CriarEInserirLogradouroAsync, reusado pelos testes de Colaborador).
    protected static string NomeCidade => SelectedDatabaseType switch
    {
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        DatabaseType.Sqlite => "SQLite",
        _ => throw new ArgumentOutOfRangeException(nameof(SelectedDatabaseType), SelectedDatabaseType, "SGBD não suportado para testes.")
    };

    protected const string NomeEstado = "SC";

    protected const string NomePais = "Brasil";

    // Requisitos da atividade 06 (Colaborador e Aluno):
    // nome = seu nome, complemento = seu sobrenome, senha contendo a sigla do SGBD.
    protected const string NomePessoa = "Pablo";

    protected const string NomeComplemento = "Valente Neto";

    protected static string SenhaTeste => $"Senha{NomeCidade}123";

    #endregion

    #region Geradores de dados aleatórios

    private static int _counter = 10000;

    protected static string GerarCep() => (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + Interlocked.Increment(ref _counter)).ToString("D8")[..8];

    // Gera 9 dígitos base únicos e calcula os dois dígitos verificadores, para que o CPF
    // passe na validação de Cpf.Criar. Um CPF só com dígitos aleatórios seria rejeitado.
    protected static string GerarCpf()
    {
        var baseCpf = (100000000L + ((DateTime.UtcNow.Ticks + Interlocked.Increment(ref _counter)) % 800000000L)).ToString("D9");

        // sequências repetidas (111.111.111-11 etc.) também são rejeitadas pelo domínio
        while (baseCpf.All(c => c == baseCpf[0])) baseCpf = (long.Parse(baseCpf) + 1).ToString("D9");

        var digitos = new int[11];
        for (var i = 0; i < 9; i++) digitos[i] = baseCpf[i] - '0';

        for (var d = 9; d < 11; d++)
        {
            var soma = 0;
            var peso = d + 1;
            for (var i = 0; i < d; i++) soma += digitos[i] * peso--;
            var resto = soma % 11;
            digitos[d] = resto < 2 ? 0 : 11 - resto;
        }

        return string.Concat(digitos);
    }

    protected static string GerarEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";

    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];

    #endregion
}
