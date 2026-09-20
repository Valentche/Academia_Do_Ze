//Pablo Valente Neto

namespace AcademiaDoZe.Domain.Enums;

// [Flags] permite combinar várias restrições em um único valor
// ex.: MatriculaRestricoes.Diabetes | MatriculaRestricoes.Alergias e assim por diante, vai que o caba tem 
// problema a rodo.
[Flags]
public enum MatriculaRestricoes
{
    None = 0,
    Diabetes = 1,
    PressaoAlta = 2,
    Labirintite = 4,
    Alergias = 8,
    ProblemasRespiratorios = 16,
    RemedioContinuo = 32
}