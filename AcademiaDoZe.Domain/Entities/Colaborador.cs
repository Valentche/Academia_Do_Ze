//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public sealed class Colaborador : Pessoa, IAggregateRoot
{
    // encapsulamento das propriedades co imutabilaide
    public DateOnly DataAdmissao { get; private set; }
    public ColaboradorTipo Tipo { get; private set; }
    public ColaboradorVinculo Vinculo { get; private set; }

    // construtor privado
    private Colaborador(int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email? email,
        Endereco endereco, Senha senha, Arquivo? foto,
        DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
        DataAdmissao = dataAdmissao;
        Tipo = tipo;
        Vinculo = vinculo;
    }

    // método de fábrica
    public static Result<Colaborador> Criar(int id, string nome, string cpf, DateOnly dataNascimento, string telefone,
        string? email, Logradouro endereco, string numero, string? complemento, string senha, Arquivo? foto,
        DateOnly dataAdmissao, ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        var notifications = new List<Notification>();

        // Validações e normalizações
        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);

        if (dataNascimento == default)
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
        else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-IdadeMinima)))
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));

        if (dataAdmissao == default)
            notifications.Add(new Notification("DataAdmissao", "DATA_ADMISSAO_OBRIGATORIO"));
        else if (dataAdmissao > DateOnly.FromDateTime(DateTime.Today))
            notifications.Add(new Notification("DataAdmissao", "DATA_ADMISSAO_MAIOR_ATUAL"));
        else if (dataNascimento != default && dataAdmissao <= dataNascimento)
            notifications.Add(new Notification("DataAdmissao", "DATA_ADMISSAO_MENOR_NASCIMENTO"));

        if (!Enum.IsDefined(tipo))
            notifications.Add(new Notification("Tipo", "TIPO_COLABORADOR_INVALIDO"));

        if (!Enum.IsDefined(vinculo))
            notifications.Add(new Notification("Vinculo", "VINCULO_COLABORADOR_INVALIDO"));

        // regra da academia, administrador só pode ser CLT
        if (Enum.IsDefined(tipo) && Enum.IsDefined(vinculo) && tipo == ColaboradorTipo.Administrador && vinculo != ColaboradorVinculo.Clt)
            notifications.Add(new Notification("Vinculo", "ADMINISTRADOR_CLT_INVALIDO"));

        // Instanciação e validação
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) notifications.AddRange(cpfResult.Notifications);

        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);

        // e-mail opcional e só valida se informado no cadatro
        Email? emailVo = null;
        if (!NormalizadoService.TextoVazioOuNulo(email))
        {
            var emailResult = Email.Criar(email!);
            if (emailResult.IsFailure) notifications.AddRange(emailResult.Notifications);
            else emailVo = emailResult.Value;
        }

        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure) notifications.AddRange(senhaResult.Notifications);

        var enderecoResult = Endereco.Criar(endereco, numero, complemento);
        if (enderecoResult.IsFailure) notifications.AddRange(enderecoResult.Notifications);

        if (notifications.Count != 0)
            return Result<Colaborador>.Failure(notifications);

        // criação e retorno do objeto
        var colaborador = new Colaborador(id, nome, cpfResult.Value!, dataNascimento, telefoneResult.Value!, emailVo,
            enderecoResult.Value!, senhaResult.Value!, foto, dataAdmissao, tipo, vinculo);

        return Result<Colaborador>.Success(colaborador);
    }

    // alteração controlada de estado
    public Result<bool> AlterarVinculo(ColaboradorTipo tipo, ColaboradorVinculo vinculo)
    {
        var notifications = new List<Notification>();

        if (!Enum.IsDefined(tipo))
            notifications.Add(new Notification("Tipo", "TIPO_COLABORADOR_INVALIDO"));

        if (!Enum.IsDefined(vinculo))
            notifications.Add(new Notification("Vinculo", "VINCULO_COLABORADOR_INVALIDO"));

        if (Enum.IsDefined(tipo) && Enum.IsDefined(vinculo) && tipo == ColaboradorTipo.Administrador && vinculo != ColaboradorVinculo.Clt)
            notifications.Add(new Notification("Vinculo", "ADMINISTRADOR_CLT_INVALIDO"));

        if (notifications.Count != 0) return Result<bool>.Failure(notifications);

        Tipo = tipo;
        Vinculo = vinculo;
        return Result<bool>.Success(true);
    }
}