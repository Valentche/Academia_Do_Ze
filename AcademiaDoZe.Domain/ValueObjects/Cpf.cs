//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public sealed record Cpf
{
    public string Valor { get; }

    // só o método de fábrica pode criar um Cpf assim garantindo que nunca exista inválido
    private Cpf(string valor)
    {
        Valor = valor;
    }

    public static Result<Cpf> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");

        // normalização: guarda somente dígitos, então "123.456.789-09" e "12345678909" são o mesmo CPF
        var textoLimpo = NormalizadoService.LimparEDigitos(valor);

        if (textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");

        // CPFs com todos os dígitos iguais (000..., 111...) são sintaticamente válidos, mas nunca existem
        if (textoLimpo.All(c => c == textoLimpo[0]))
            return Result<Cpf>.Failure("Cpf", "CPF_INVALIDO");

        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }

    public override string ToString() => Valor;
}