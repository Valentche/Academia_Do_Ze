using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoService
{
    Task<AcessoResultadoDto> RegistrarAcessoAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<AcessoResultadoDto> RegistrarAcessoColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<bool> AlunoEstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<bool> ColaboradorEstaNaAcademiaAsync(int colaboradorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AcessoHistoricoItemDto>> ObterAcessosHojeAsync(CancellationToken cancellationToken = default);
    Task<TimeSpan> ObterHorasTrabalhadasHojeColaboradorAsync(int colaboradorId, CancellationToken cancellationToken = default);
}
