//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Entities;

public sealed class AcessoAluno : Entity, IAggregateRoot
{
    public Aluno Aluno { get; private set; }
    public DateTime DataHoraEntrada { get; private set; }
    public DateTime? DataHoraSaida { get; private set; }

    // construtor privado
    private AcessoAluno(
        int id,
        Aluno aluno,
        DateTime dataHoraEntrada,
        DateTime? dataHoraSaida)
        : base(id)
    {
        Aluno = aluno;
        DataHoraEntrada = dataHoraEntrada;
        DataHoraSaida = dataHoraSaida;
    }

    // ponto de entrada para criar um objeto válido
    public static Result<AcessoAluno> Criar(int id, Aluno aluno, DateTime dataHoraEntrada, DateTime? dataHoraSaida = null)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_OBRIGATORIO"));

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
            return Result<AcessoAluno>.Failure(notifications);

        return Result<AcessoAluno>.Success(new AcessoAluno(id, aluno!, dataHoraEntrada, dataHoraSaida));
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