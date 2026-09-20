//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Domain.Tests.Common;

public class NotificationTests
{
    [Fact(DisplayName = "Notification: dois registros com os mesmos valores são iguais (semântica de record)")]
    public void Deve_Ser_Igual_Quando_MesmosValores()
    {
        var a = new Notification("Cpf", "CPF_OBRIGATORIO");
        var b = new Notification("Cpf", "CPF_OBRIGATORIO");

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact(DisplayName = "Notification: registros com valores diferentes não são iguais")]
    public void Deve_Ser_Diferente_Quando_ValoresDiferentes()
    {
        var a = new Notification("Cpf", "CPF_OBRIGATORIO");
        var b = new Notification("Email", "EMAIL_FORMATO");

        Assert.NotEqual(a, b);
    }
}