//Pablo Valente Neto

namespace AcademiaDoZe.Domain.Exceptions;

public sealed class DomainException(string message) : Exception(message)
{
}