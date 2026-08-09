//Pablo Valente Neto

using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Entities;

// Classe base para todas as entidades, garantindo identidade única e validação de Id.
// da pra usar o Fail-Fast, um Id negativo é um erro de programação,
// não um erro de preenchimento do usuário, portanto não faz sentido acumular notificação.
public abstract class Entity
{
    public int Id { get; protected set; }

    protected Entity(int id = 0)
    {
        if (id < 0) throw new DomainException("ID_NEGATIVO");
        Id = id;
    }
}