namespace AcademiaDoZe.Application.DTOs;

public enum TipoAcessoResultado
{
    EntradaAutorizada,
    SaidaRegistrada,
    AcessoNegado
}

public class AcessoResultadoDto
{
    public bool Sucesso { get; set; }
    public TipoAcessoResultado TipoResultado { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public bool EhEntrada { get; set; }
    public TimeSpan? TempoPermanencia { get; set; }
    public int? DiasRestantesPlano { get; set; }
    public TimeSpan? HorasTrabalhadasHoje { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
}
