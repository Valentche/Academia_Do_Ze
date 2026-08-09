//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public sealed class Aluno : Pessoa, IAggregateRoot
{
    // construtor privado
    private Aluno(
        int id,
        string nome,
        Cpf cpf,
        DateOnly dataNascimento,
        Telefone telefone,
        Email? email,
        Endereco endereco,
        Senha senha,
        Arquivo? foto)
        : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
    }

    public static Result<Aluno> Criar(
        int id,
        string nome,
        string cpf,
        DateOnly dataNascimento,
        string telefone,
        string? email,
        Logradouro endereco,
        string numero,
        string? complemento,
        string senha,
        Arquivo? foto = null)
    {
        var notifications = new List<Notification>();

        // Validações e normalizações
        if (NormalizadoService.TextoVazioOuNulo(nome))
            notifications.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
        else
            nome = NormalizadoService.LimparEspacos(nome);

        if (dataNascimento == default)
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
        else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today))
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_MAIOR_ATUAL"));
        else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-IdadeMinima)))
            notifications.Add(new Notification("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));

        // instanciação e validação
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) notifications.AddRange(cpfResult.Notifications);

        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);

        // o e-mail é opcional
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
            return Result<Aluno>.Failure(notifications);

        // criação e retorno do objeto
        var aluno = new Aluno(id, nome, cpfResult.Value!, dataNascimento, telefoneResult.Value!, emailVo,
            enderecoResult.Value!, senhaResult.Value!, foto);

        return Result<Aluno>.Success(aluno);
    }
}