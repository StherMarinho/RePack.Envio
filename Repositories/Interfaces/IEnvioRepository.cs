using ApiEnvio.DTOs;
using ApiEnvio.Models;
 
namespace ApiEnvio.Repositories.Interfaces;
 
public interface IEnvioRepository
{
    Task<int> CriarAsync(Envio envio);
    Task<Envio?> ObterPorIdAsync(int id);
    Task<EnvioDetalhadoDto?> ObterDetalhadoPorIdAsync(int id);
    Task<PagedResultDto<EnvioResumoDto>> ListarAsync(FiltroEnvioDto filtro);
    Task AtualizarStatusAsync(int id, int idStatus, string? observacao);
    Task<bool> ExisteAsync(int id);
    Task ExcluirAsync(int id);
Task EditarAsync(int id, EditarEnvioDto dto);
}