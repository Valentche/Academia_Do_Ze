//Pablo Valente Neto

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    private const string BaseSelect = "SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 1";

    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string insertSql = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");
            await using var command = await CreateCommandAsync(insertSql, cancellationToken);
            command.AddParameter("@PessoaTipo", 1, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int id = await command.ExecuteScalarIdAsync("ERRO_INSERIR_ACESSO_COLABORADOR", "Erro ao registrar acesso do colaborador no banco de dados.", cancellationToken);
            return AcessoColaborador.Carregar(id, entity.ColaboradorId, entity.DataHora);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_INSERIR_ACESSO_COLABORADOR", $"Erro ao registrar acesso do colaborador ID {entity.ColaboradorId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
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
            throw new InfrastructureException("ERRO_OBTER_ACESSO_COLABORADOR", $"Erro ao obter acesso do colaborador ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelect} ORDER BY data_hora DESC, id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS_ACESSOS_COLABORADOR", $"Erro ao obter acessos de colaboradores: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora.ToString("yyyy-MM-dd HH:mm:ss"), DbType.String);

            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Acesso de colaborador com ID {entity.Id} não encontrado.");

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_COLABORADOR", $"Erro ao atualizar acesso do colaborador ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            int rows = await command.ExecuteNonQueryAsync(cancellationToken);
            return rows > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_COLABORADOR", $"Erro ao remover acesso do colaborador ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelect} AND pessoa_id = @ColaboradorId ORDER BY data_hora DESC, id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                return Map(reader);
            }
            return null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO_COLABORADOR", $"Erro ao obter último acesso do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = $"{BaseSelect}";
            if (colaboradorId.HasValue)
                sql += " AND pessoa_id = @ColaboradorId";

            if (inicio.HasValue)
                sql += " AND data_hora >= @Inicio";

            if (fim.HasValue)
                sql += " AND data_hora <= @Fim";

            sql += " ORDER BY data_hora ASC, id_acesso ASC";

            await using var command = await CreateCommandAsync(sql, cancellationToken);
            if (colaboradorId.HasValue)
                command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);

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
            var lista = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                lista.Add(Map(reader));
            }
            return lista;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_COLABORADOR_PERIODO", $"Erro ao obter acessos do colaborador por período: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var acessosHoje = (await ObterAcessosPorColaboradorPeriodo(colaboradorId, hoje, hoje, cancellationToken)).ToList();
            return acessosHoje.Count % 2 == 1;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_ESTA_NA_ACADEMIA_COLABORADOR", $"Erro ao verificar se o colaborador {colaboradorId} está na academia: {ex.Message}", ex);
        }
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        try
        {
            var acessos = (await ObterAcessosPorColaboradorPeriodo(colaboradorId, data, data, cancellationToken))
                .OrderBy(a => a.DataHora)
                .ToList();

            TimeSpan total = TimeSpan.Zero;
            for (int i = 0; i < acessos.Count; i += 2)
            {
                if (i + 1 < acessos.Count)
                {
                    total += (acessos[i + 1].DataHora - acessos[i].DataHora);
                }
                else
                {
                    // Caso ainda esteja dentro (número ímpar de batidas)
                    if (data == DateOnly.FromDateTime(DateTime.Today))
                    {
                        var agora = DateTime.Now;
                        if (agora > acessos[i].DataHora)
                            total += (agora - acessos[i].DataHora);
                    }
                }
            }
            return total;
        }
        catch (Exception ex)
        {
            throw new InfrastructureException("ERRO_OBTER_HORAS_TRABALHADAS", $"Erro ao calcular horas trabalhadas do colaborador {colaboradorId}: {ex.Message}", ex);
        }
    }

    private static AcessoColaborador Map(DbDataReader reader)
    {
        int id = reader.GetInt32Value("id_acesso");
        int colaboradorId = reader.GetInt32Value("pessoa_id");
        DateTime dataHora = reader.GetDateTimeValue("data_hora");
        return AcessoColaborador.Carregar(id, colaboradorId, dataHora);
    }
}
