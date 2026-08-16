//Pablo Valente Neto

using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory(DisplayName = "Email: formatos válidos")]
    [InlineData("user@example.com")]
    [InlineData("user.name+tag@sub.example.com")]
    public void Deve_Criar_Email_Quando_Valido(string input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(input, result.Value!.Valor);
    }

    [Theory(DisplayName = "Email: nulo/vazio/espaços -> EMAIL_FORMATO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_EmailNuloOuVazio(string? input)
    {
        var result = Email.Criar(input!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory(DisplayName = "Email: formato inválido -> EMAIL_FORMATO")]
    [InlineData("usuarioSemArroba.com")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@dominio")]
    [InlineData("usuario@.dominio.com")]
    public void Deve_Falhar_Criacao_Quando_FormatoInvalido(string input)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "EMAIL_FORMATO");
    }

    [Theory(DisplayName = "Email: remove espaços extras (trim) quando input tem espaços")]
    [InlineData(" user@example.com ", "user@example.com")]
    [InlineData("  user@example.com  ", "user@example.com")]
    public void Deve_Criar_Email_E_RemoverEspacos_Quando_InputTemEspacos(string input, string expected)
    {
        var result = Email.Criar(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
    }
}