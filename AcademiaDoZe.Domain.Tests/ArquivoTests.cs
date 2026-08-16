//Pablo Valente Neto

using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ArquivoTests
{
    [Theory(DisplayName = "Arquivo: conteúdo válido é aceito")]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(1024 * 1024)]
    public void Deve_Criar_Arquivo_Quando_ConteudoValido(int tamanho)
    {
        var conteudo = new byte[tamanho];

        var result = Arquivo.Criar(conteudo);

        Assert.True(result.IsSuccess);
        Assert.Equal(tamanho, result.Value!.Conteudo.Length);
    }

    [Fact(DisplayName = "Arquivo: conteúdo nulo -> ARQUIVO_OBRIGATORIO")]
    public void Deve_Falhar_Criacao_Quando_ConteudoNulo()
    {
        var result = Arquivo.Criar(null!);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_OBRIGATORIO");
    }

    [Theory(DisplayName = "Arquivo: valida limite máximo de 15MB -> ARQUIVO_TIPO_TAMANHO")]
    [InlineData(15 * 1024 * 1024, true)]
    [InlineData(15 * 1024 * 1024 + 1, false)]
    public void Deve_Validar_LimiteDeTamanhoMaximo(int tamanho, bool expectSuccess)
    {
        var conteudo = new byte[tamanho];

        var result = Arquivo.Criar(conteudo);

        Assert.Equal(expectSuccess, result.IsSuccess);
        if (!expectSuccess)
            Assert.Contains(result.Notifications, n => n.Mensagem == "ARQUIVO_TIPO_TAMANHO");
    }
}