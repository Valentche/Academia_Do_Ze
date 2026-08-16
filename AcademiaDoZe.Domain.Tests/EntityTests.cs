//Pablo Valente Neto

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class EntityTests
{
    // entity é abstrata, então uma implementação mínima só para exercitar a base la
    private sealed class EntidadeFake : Entity
    {
        public EntidadeFake(int id) : base(id) { }
    }

    [Theory(DisplayName = "Entity: Id negativo lança DomainException (fail-fast)")]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Deve_Lancar_DomainException_Quando_IdNegativo(int id)
    {
        var exception = Assert.Throws<DomainException>(() => new EntidadeFake(id));

        Assert.Equal("ID_NEGATIVO", exception.Message);
    }

    [Theory(DisplayName = "Entity: Id válido (>= 0) é atribuído corretamente")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public void Deve_Atribuir_Id_Quando_IdValido(int id)
    {
        var entidade = new EntidadeFake(id);

        Assert.Equal(id, entidade.Id);
    }
}