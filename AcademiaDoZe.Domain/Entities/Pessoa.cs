//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

// Classe base das pessoas do domínio
// quem é persistido são as classes concretas.
public abstract class Pessoa : Entity
{
    // Idade mínima exigida pela academia para cadastro
    public const int IdadeMinima = 12;

    // o estado só muda por métodos controlados da própria entidade
    public string Nome { get; protected set; }
    public Cpf Cpf { get; protected set; }
    public DateOnly DataNascimento { get; protected set; }
    public Telefone Telefone { get; protected set; }
    public Email? Email { get; protected set; }
    public Endereco Endereco { get; protected set; }
    public Senha Senha { get; protected set; }
    public Arquivo? Foto { get; protected set; }

    protected Pessoa(
        int id,
        string nome,
        Cpf cpf,
        DateOnly dataNascimento,
        Telefone telefone,
        Email? email,
        Endereco endereco,
        Senha senha,
        Arquivo? foto) : base(id)
    {
        Nome = nome;
        Cpf = cpf;
        DataNascimento = dataNascimento;
        Telefone = telefone;
        Email = email;
        Endereco = endereco;
        Senha = senha;
        Foto = foto;
    }

    // idade calculada, não armazenada (evita dado inconsistente dessa forma)
    public int Idade(DateOnly? referencia = null)
    {
        var hoje = referencia ?? DateOnly.FromDateTime(DateTime.Today);
        var idade = hoje.Year - DataNascimento.Year;
        if (DataNascimento > hoje.AddYears(-idade)) idade--;
        return idade;
    }

    // métodos controlados de ateraçõa

    public Result<bool> AlterarSenha(string novaSenha)
    {
        var senhaResult = Senha.Criar(novaSenha);
        if (senhaResult.IsFailure) return Result<bool>.Failure(senhaResult.Notifications);

        Senha = senhaResult.Value!;
        return Result<bool>.Success(true);
    }

    public Result<bool> AtualizarContato(string telefone, string? email)
    {
        var notifications = new List<Notification>();

        var telefoneResult = Telefone.Criar(telefone);
        if (telefoneResult.IsFailure) notifications.AddRange(telefoneResult.Notifications);

        Email? emailVo = null;
        if (!Services.NormalizadoService.TextoVazioOuNulo(email))
        {
            var emailResult = Email.Criar(email!);
            if (emailResult.IsFailure) notifications.AddRange(emailResult.Notifications);
            else emailVo = emailResult.Value;
        }

        if (notifications.Count != 0) return Result<bool>.Failure(notifications);

        Telefone = telefoneResult.Value!;
        Email = emailVo;
        return Result<bool>.Success(true);
    }

    public void AtualizarFoto(Arquivo? foto) => Foto = foto;

    public void AtualizarEndereco(Endereco endereco) => Endereco = endereco;
}