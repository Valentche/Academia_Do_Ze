//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;
namespace AcademiaDoZe.Domain.Tests.Common;

public class ResultTests
{
    [Fact(DisplayName = "Result: Success -> IsSuccess true, sem notificações, mantém o Value")]
    public void Deve_Retornar_Sucesso_Quando_Success()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Empty(result.Notifications);
    }

    [Fact(DisplayName = "Result: Failure(propriedade, mensagem) -> IsFailure true, uma notificação")]
    public void Deve_Retornar_Falha_Quando_FailureComPropriedadeEMensagem()
    {
        var result = Result<string>.Failure("Campo", "MENSAGEM_TESTE");

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Single(result.Notifications);
        Assert.Equal("Campo", result.Notifications.First().Propriedade);
        Assert.Equal("MENSAGEM_TESTE", result.Notifications.First().Mensagem);
    }

    [Fact(DisplayName = "Result: Failure(Notification) -> propaga a notificação informada")]
    public void Deve_Retornar_Falha_Quando_FailureComNotification()
    {
        var notification = new Notification("Prop", "Msg");

        var result = Result<int>.Failure(notification);

        Assert.True(result.IsFailure);
        Assert.Single(result.Notifications);
        Assert.Equal(notification, result.Notifications.First());
    }

    [Fact(DisplayName = "Result: Failure(lista) -> acumula todas as notificações")]
    public void Deve_Retornar_Falha_Quando_FailureComListaDeNotifications()
    {
        var notifications = new List<Notification>
        {
            new("A", "MsgA"),
            new("B", "MsgB")
        };

        var result = Result<int>.Failure(notifications);

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Notifications.Count);
        Assert.Contains(result.Notifications, n => n.Mensagem == "MsgA");
        Assert.Contains(result.Notifications, n => n.Mensagem == "MsgB");
    }
}