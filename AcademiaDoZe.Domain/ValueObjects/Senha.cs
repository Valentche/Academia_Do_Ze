//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public sealed record Senha
{
    public const int TamanhoMinimo = 6;

    public string Valor { get; }

    // só o método de fábrica pode criar uma Senha
    private Senha(string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string valor)
    {
        var notifications = new List<Notification>();

        // Atenção: senha NÃO é normalizada (remover espaços mudaria o valor digitado pelo usuário)
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Senha>.Failure("Senha", "SENHA_OBRIGATORIO");

        if (valor.Length < TamanhoMinimo)
            notifications.Add(new Notification("Senha", "SENHA_TAMANHO_MINIMO"));

        if (!valor.Any(char.IsLetter) || !valor.Any(char.IsDigit))
            notifications.Add(new Notification("Senha", "SENHA_COMPLEXIDADE"));

        if (notifications.Count != 0)
            return Result<Senha>.Failure(notifications);

        return Result<Senha>.Success(new Senha(valor));
    }

    // nunca expor a senha em log/ToString, censurando e impedindo visulização invalida
    public override string ToString() => "********";
}