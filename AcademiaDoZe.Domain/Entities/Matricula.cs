//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public sealed class Matricula : Entity, IAggregateRoot
{
    // máscaras com todas as restrições válidas
    private const MatriculaRestricoes TodasRestricoes =
        MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta | MatriculaRestricoes.Labirintite |
        MatriculaRestricoes.Alergias | MatriculaRestricoes.ProblemasRespiratorios | MatriculaRestricoes.RemedioContinuo;

    // encapsulamento das propriedades, com imutavel on
    public Aluno Aluno { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFinal { get; private set; }
    public string Objetivo { get; private set; }
    public MatriculaRestricoes Restricoes { get; private set; }
    public string ObservacoesRestricoes { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }

    // construtor privado
    private Matricula(
        int id,
        Aluno aluno,
        MatriculaPlano plano,
        DateOnly dataInicio,
        DateOnly dataFinal,
        string objetivo,
        MatriculaRestricoes restricoes,
        string observacoesRestricoes,
        Arquivo? laudoMedico)
        : base(id)
    {
        Aluno = aluno;
        Plano = plano;
        DataInicio = dataInicio;
        DataFinal = dataFinal;
        Objetivo = objetivo;
        Restricoes = restricoes;
        ObservacoesRestricoes = observacoesRestricoes;
        LaudoMedico = laudoMedico;
    }

    // método de fábrica
    public static Result<Matricula> Criar(
        int id,
        Aluno aluno,
        MatriculaPlano plano,
        DateOnly dataInicio,
        DateOnly dataFinal,
        string objetivo,
        MatriculaRestricoes restricoes = MatriculaRestricoes.None,
        string? observacoesRestricoes = null,
        Arquivo? laudoMedico = null)
    {
        var notifications = new List<Notification>();

        if (aluno == null)
            notifications.Add(new Notification("Aluno", "ALUNO_OBRIGATORIO"));

        if (!Enum.IsDefined(plano))
            notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));

        if (dataInicio == default)
            notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIO"));

        if (dataFinal == default)
            notifications.Add(new Notification("DataFinal", "DATA_FINAL_OBRIGATORIO"));
        else if (dataInicio != default && dataFinal <= dataInicio)
            notifications.Add(new Notification("DataFinal", "DATA_FINAL_MENOR_INICIO"));

        if (NormalizadoService.TextoVazioOuNulo(objetivo))
            notifications.Add(new Notification("Objetivo", "OBJETIVO_OBRIGATORIO"));
        else
            objetivo = NormalizadoService.LimparEspacos(objetivo);

        // combinação de flags inválida (algum bit que não pertence ao enum)
        if ((restricoes & ~TodasRestricoes) != 0)
            notifications.Add(new Notification("Restricoes", "RESTRICOES_INVALIDA"));

        var observacoesLimpas = NormalizadoService.LimparEspacos(observacoesRestricoes);

        // regra de negócio: havendo restrição médica, laudo e observações passam a ser obrigatórios
        if (restricoes != MatriculaRestricoes.None)
        {
            if (laudoMedico == null)
                notifications.Add(new Notification("LaudoMedico", "LAUDO_MEDICO_OBRIGATORIO"));

            if (string.IsNullOrWhiteSpace(observacoesLimpas))
                notifications.Add(new Notification("ObservacoesRestricoes", "OBSERVACOES_RESTRICOES_OBRIGATORIO"));
        }

        if (notifications.Count != 0)
            return Result<Matricula>.Failure(notifications);

        var matricula = new Matricula(id, aluno!, plano, dataInicio, dataFinal, objetivo, restricoes, observacoesLimpas, laudoMedico);

        return Result<Matricula>.Success(matricula);
    }

    // calcula a data final a partir do plano contratado
    public static DateOnly CalcularDataFinal(MatriculaPlano plano, DateOnly dataInicio) => plano switch
    {
        MatriculaPlano.Mensal => dataInicio.AddMonths(1),
        MatriculaPlano.Trimestral => dataInicio.AddMonths(3),
        MatriculaPlano.Semestral => dataInicio.AddMonths(6),
        MatriculaPlano.Anual => dataInicio.AddYears(1),
        _ => dataInicio
    };

    // comportamentos do domínio
    public bool PossuiRestricoes => Restricoes != MatriculaRestricoes.None;

    public bool EstaAtivaEm(DateOnly? data = null)
    {
        var referencia = data ?? DateOnly.FromDateTime(DateTime.Today);
        return referencia >= DataInicio && referencia <= DataFinal;
    }

    // renovação controlada
    public Result<bool> Renovar(MatriculaPlano plano, DateOnly novaDataInicio)
    {
        var notifications = new List<Notification>();

        if (!Enum.IsDefined(plano))
            notifications.Add(new Notification("Plano", "PLANO_INVALIDO"));

        if (novaDataInicio == default)
            notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIO"));
        else if (novaDataInicio < DataInicio)
            notifications.Add(new Notification("DataInicio", "DATA_INICIO_MENOR_ATUAL"));

        if (notifications.Count != 0) return Result<bool>.Failure(notifications);

        Plano = plano;
        DataInicio = novaDataInicio;
        DataFinal = CalcularDataFinal(plano, novaDataInicio);
        return Result<bool>.Success(true);
    }
}