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

    protected string NomeCidade => DatabaseType switch
    {
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        DatabaseType.Sqlite => "SQLite",
        _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
    };

    protected const string NomeEstado = "SC";

    protected const string NomePais = "Brasil";

    #endregion

    #region Geradores de dados aleatórios

    private static int _counter = 10000;

    protected static string GerarCep() => (80000000 + ((int)(DateTime.UtcNow.Ticks % 8000000)) + Interlocked.Increment(ref _counter)).ToString("D8")[..8];

    protected static string GerarCpf() => (10000000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];

    protected static string GerarEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@test.com";

    protected static string GerarTelefone() => (49990000000L + ((DateTime.UtcNow.Ticks % 8000000000L)) + Interlocked.Increment(ref _counter)).ToString("D11")[..11];

    #endregion
}
