//Pablo Valente Neto

using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.ValueObjects;

public sealed record Arquivo
{
    public const int TamanhoMaximoBytes = 15 * 1024 * 1024; // 15MB

    public byte[] Conteudo { get; }

    private Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo;
    }

    public static Result<Arquivo> Criar(byte[] conteudo)
    {
        if (conteudo == null || conteudo.Length == 0)
            return Result<Arquivo>.Failure("Arquivo", "ARQUIVO_OBRIGATORIO");

        if (conteudo.Length > TamanhoMaximoBytes)
            return Result<Arquivo>.Failure("Arquivo", "ARQUIVO_TIPO_TAMANHO");

        // cria e retorna o objeto
        return Result<Arquivo>.Success(new Arquivo(conteudo));
    }

    // (NAO ESQUECER MARRECO) Value Object = igualdade por valor posto. O record compara byte[] por referência,
    // dessa forma a comparação é reescrita para considerar o conteúdo do arquivo.
    public bool Equals(Arquivo? other) => other is not null && Conteudo.AsSpan().SequenceEqual(other.Conteudo);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(Conteudo);
        return hash.ToHashCode();
    }
}