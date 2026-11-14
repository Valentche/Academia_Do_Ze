//Pablo Valente Neto

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private const string BaseSelect = "SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 0";

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string insertSql = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");
            await using var command = await CreateCommandAsync(insertSql, cancellationToken);
            command.AddParameter("@PessoaTipo", 0, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int id = await command.ExecuteScalarIdAsync("ERRO_INSERIR_ACESSO_ALUNO", "Erro ao registrar acesso do aluno no banco de dados.", cancellationToken);
            return AcessoAluno.Carregar(id, entity.AlunoId, entity.DataHora);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_INSERIR_ACESSO_ALUNO", $"Erro ao registrar acesso do aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelect} AND id_acesso = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return Map(reader);
            }
            return null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSO_ALUNO", $"Erro ao obter acesso do aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelect} ORDER BY data_hora DESC, id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var lista = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS_ACESSOS_ALUNO", $"Erro ao obter acessos de alunos: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = 0";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Acesso de aluno com ID {entity.Id} não encontrado.");

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", $"Erro ao atualizar acesso do aluno ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 0";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", $"Erro ao remover acesso do aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelect} AND pessoa_id = @AlunoId ORDER BY data_hora DESC, id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return Map(reader);
            }
            return null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO_ALUNO", $"Erro ao obter último acesso do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = $"{BaseSelect}";
            if (alunoId.HasValue)
                sql += " AND pessoa_id = @AlunoId";

            if (inicio.HasValue)
                sql += " AND data_hora >= @Inicio";

            if (fim.HasValue)
                sql += " AND data_hora <= @Fim";

            sql += " ORDER BY data_hora ASC, id_acesso ASC";

            await using var command = await CreateCommandAsync(sql, cancellationToken);
            if (alunoId.HasValue)
                command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);

            if (inicio.HasValue)
            {
                var dtInicio = inicio.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd HH:mm:ss");
                command.AddParameter("@Inicio", dtInicio, DbType.String);
            }

            if (fim.HasValue)
            {
                var dtFim = fim.Value.ToDateTime(new TimeOnly(23, 59, 59)).ToString("yyyy-MM-dd HH:mm:ss");
                command.AddParameter("@Fim", dtFim, DbType.String);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var lista = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_ALUNO_PERIODO", $"Erro ao obter acessos do aluno por período: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var acessosHoje = (await ObterAcessosPorAlunoPeriodo(alunoId, hoje, hoje, cancellationToken)).ToList();
            return acessosHoje.Count % 2 == 1;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_ESTA_NA_ACADEMIA", $"Erro ao verificar se o aluno {alunoId} está na academia: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            var todos = await ObterTodos(cancellationToken);
            return todos
                .Where(a => a.DataHora.Month == mes)
                .GroupBy(a => new TimeOnly(a.DataHora.Hour, 0))
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("ERRO_OBTER_HORARIO_MAIS_PROCURADO", $"Erro ao calcular horário mais procurado no mês {mes}: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            var todos = (await ObterTodos(cancellationToken))
                .Where(a => a.DataHora.Month == mes)
                .OrderBy(a => a.AlunoId)
                .ThenBy(a => a.DataHora)
                .ToList();

            var permanencias = new List<TimeSpan>();
            for (int i = 0; i < todos.Count - 1; i += 2)
            {
                if (todos[i].AlunoId == todos[i + 1].AlunoId)
                {
                    var duracao = todos[i + 1].DataHora - todos[i].DataHora;
                    if (duracao > TimeSpan.Zero && duracao < TimeSpan.FromHours(8))
                        permanencias.Add(duracao);
                }
            }

            var media = permanencias.Count != 0
                ? TimeSpan.FromTicks((long)permanencias.Average(p => p.Ticks))
                : TimeSpan.Zero;

            return new Dictionary<int, TimeSpan> { { mes, media } };
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("ERRO_OBTER_PERMANENCIA_MEDIA", $"Erro ao obter permanência média no mês {mes}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            using var alunoRepo = new AlunoRepository(_connectionString, _databaseType);
            var todosAlunos = (await alunoRepo.ObterTodos(cancellationToken)).ToList();
            var dataLimite = DateTime.Today.AddDays(-dias);

            var acessosRecentes = (await ObterTodos(cancellationToken))
                .Where(a => a.DataHora >= dataLimite)
                .Select(a => a.AlunoId)
                .ToHashSet();

            return todosAlunos.Where(a => !acessosRecentes.Contains(a.Id));
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("ERRO_ALUNOS_SEM_ACESSO", $"Erro ao obter alunos sem acesso nos últimos {dias} dias: {ex.Message}", ex);
        }
    }

    private static AcessoAluno Map(DbDataReader reader)
    {
        int id = reader.GetInt32Value("id_acesso");
        int alunoId = reader.GetInt32Value("pessoa_id");
        DateTime dataHora = reader.GetDateTimeValue("data_hora");
        return AcessoAluno.Carregar(id, alunoId, dataHora);
    }
}
