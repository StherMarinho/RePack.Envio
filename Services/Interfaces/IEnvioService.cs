using ApiEnvio.DTOs;

namespace ApiEnvio.Services.Interfaces;

public interface IEnvioService
{
    Task<ApiResponseDto<EnvioDetalhadoDto>> CriarEnvioAsync(CriarEnvioDto dto);
    Task<ApiResponseDto<EnvioDetalhadoDto>> ObterEnvioAsync(int id);
    Task<ApiResponseDto<PagedResultDto<EnvioResumoDto>>> ListarEnviosAsync(FiltroEnvioDto filtro);
    Task<ApiResponseDto<EnvioDetalhadoDto>> AvaliarEnvioAsync(int id, AvaliarEnvioDto dto);
    Task<ApiResponseDto<bool>> CancelarEnvioAsync(int id);
    Task<ApiResponseDto<bool>> ExcluirEnvioAsync(int id);
    Task<ApiResponseDto<EnvioDetalhadoDto>> EditarEnvioAsync(int id, EditarEnvioDto dto);
}