namespace AcademiaDoZe.Application.DTOs;

public class AcessoHistoricoItemDto
{
    public int Id { get; set; }
    public int PessoaId { get; set; }
    public string NomePessoa { get; set; } = string.Empty;
    public string CpfPessoa { get; set; } = string.Empty;
    public byte[]? FotoPessoa { get; set; }
    public bool EhAluno { get; set; }
    public bool EhEntrada { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string HoraFormatada => DataHora.ToString("HH:mm:ss");
}
