//Pablo Valente Neto

namespace AcademiaDoZe.Infrastructure.Exceptions;

// Exceção personalizada para erros específicos da camada de infraestrutura.
// Encapsula qualquer falha no nível de acesso a dados: problemas de conexão,
// erros na criação de comandos, scripts não encontrados ou erros de SGBD.
public class InfrastructureException : Exception
{
    // Identificador textual do erro, ex: "CONEXAO_STRING_VAZIA", "FALHA_CRIAR_COMANDO".
    public string? ErrorCode { get; }

    // Registro exato da data/hora em UTC no momento em que a exceção foi disparada.
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public InfrastructureException(string message) : base(message) { }

    public InfrastructureException(string message, Exception innerException) : base(message, innerException) { }

    public InfrastructureException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public InfrastructureException(string errorCode, string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public override string ToString()
    {
        var codeInfo = string.IsNullOrWhiteSpace(ErrorCode) ? "" : $" [{ErrorCode}]";
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss UTC}]{codeInfo} {base.ToString()}";
    }
}
