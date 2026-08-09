//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public sealed class AcessoColaborador : Entity, IAggregateRoot
{
    public Colaborador Colaborador { get; private set; }
    public DateTime DataHoraEntrada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    // construtor privado
    private AcessoColaborador(
        int id,
        Colaborador colaborador,
        DateTime dataHoraEntrada,
        DateTime? dataHoraSaida)
        : base(id)
    {
        Colaborador = colaborador;
        DataHoraEntrada = dataHoraEntrada;
        DataHoraSaida = dataHoraSaida;
    }

    // ponto de entrada para criar um objeto válido
    public static Result<AcessoColaborador> Criar(int id, Colaborador colaborador, DateTime dataHoraEntrada, DateTime? dataHoraSaida = null)
    {
        var notifications = new List<Notification>();

        if (colaborador == null)
            notifications.Add(new Notification("Colaborador", "COLABORADOR_OBRIGATORIO"));

        if (dataHoraEntrada == default)
            notifications.Add(new Notification("DataHoraEntrada", "DATA_HORA_ENTRADA_OBRIGATORIO"));
        else if (dataHoraEntrada > DateTime.Now)
            notifications.Add(new Notification("DataHoraEntrada", "DATA_HORA_ENTRADA_FUTURA"));

        if (dataHoraSaida.HasValue)
        {
            if (dataHoraSaida.Value > DateTime.Now)
                notifications.Add(new Notification("DataHoraSaida", "DATA_HORA_SAIDA_FUTURA"));
            else if (dataHoraSaida.Value <= dataHoraEntrada)
                notifications.Add(new Notification("DataHoraSaida", "DATA_HORA_SAIDA_MENOR_ENTRADA"));
        }

        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        return Result<AcessoColaborador>.Success(new AcessoColaborador(id, colaborador!, dataHoraEntrada, dataHoraSaida));
    }

    public bool EmAndamento => DataHoraSaida is null;

    public TimeSpan? Permanencia => DataHoraSaida.HasValue ? DataHoraSaida.Value - DataHoraEntrada : null;

    // a saída só pode ser registrada uma vez e após a entrada
    public Result<bool> RegistrarSaida(DateTime dataHoraSaida)
    {
        if (DataHoraSaida.HasValue)
            return Result<bool>.Failure("DataHoraSaida", "SAIDA_JA_REGISTRADA");

        if (dataHoraSaida > DateTime.Now)
            return Result<bool>.Failure("DataHoraSaida", "DATA_HORA_SAIDA_FUTURA");

        if (dataHoraSaida <= DataHoraEntrada)
            return Result<bool>.Failure("DataHoraSaida", "DATA_HORA_SAIDA_MENOR_ENTRADA");

        DataHoraSaida = dataHoraSaida;
        return Result<bool>.Success(true);
    }
}