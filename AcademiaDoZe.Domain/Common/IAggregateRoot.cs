//Pablo Valente Neto

namespace AcademiaDoZe.Domain.Common;
/// <summary>
/// Interface marcadora (marker interface) usada para identificar as Raízes de Agregado.
/// Somente entidades que implementam IAggregateRoot poderão ser persistidas/consultadas
/// diretamente pelos repositórios (regra do DDD: o repositório é por agregado, não por entidade).
/// </summary>
public interface IAggregateRoot
{
}