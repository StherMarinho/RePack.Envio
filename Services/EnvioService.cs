using ApiEnvio.DTOs;
using ApiEnvio.Models;
using ApiEnvio.Repositories.Interfaces;
using ApiEnvio.Services.Interfaces;

namespace ApiEnvio.Services;

public class EnvioService : IEnvioService
{
    private readonly IEnvioRepository _envioRepo;
    private readonly IItemEnvioRepository _itemRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EnvioService> _logger;

    //IDs de status conforme a tabela Status_envio no banco
    private const int STATUS_PENDENTE = 1;
    private const int STATUS_CONCLUIDO = 2;
    private const int STATUS_CANCELADO = 3;

    public EnvioService(IEnvioRepository envioRepo, IItemEnvioRepository itemRepo, IHttpClientFactory httpClientFactory, ILogger<EnvioService> logger)
    {
        _envioRepo = envioRepo;
        _itemRepo = itemRepo;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ApiResponseDto<EnvioDetalhadoDto>> CriarEnvioAsync(CriarEnvioDto dto)
    {
        //Validar se todas as embalagens existem e estão ativas
        foreach (var idEmbalagem in dto.IdsEmbalagens)
        {
            var existe = await _itemRepo.EmbalagemExisteEAtivaAsync(idEmbalagem);

            if (!existe)
            return ApiResponseDto<EnvioDetalhadoDto>.Erro($"A embalagem de ID {idEmbalagem} não existe ou está inativa.");
        }

        var envio = new Envio
        {
          IdUsuario = dto.IdUsuario,
          IdEmpresa = dto.IdEmpresa,
          DataEnvio = DateTime.UtcNow,
          IdStatusEnvio = STATUS_PENDENTE,
          Observacao = dto.Observacao,
          QuantidadeItens = dto.IdsEmbalagens.Count  
        };

        try
        {
            var idEnvio = await _envioRepo.CriarAsync(envio);
            await _itemRepo.InserirItensAsync(idEnvio, dto.IdsEmbalagens);

            var detalhe = await _envioRepo.ObterDetalhadoPorIdAsync(idEnvio);
            return ApiResponseDto<EnvioDetalhadoDto>.Ok(detalhe!, "Envio registrado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar envio para o usuário {IdUsuario}", dto.IdUsuario);
            return ApiResponseDto<EnvioDetalhadoDto>.Erro("Erro interno ao registrar envio.");
        }

    }

    public async Task<ApiResponseDto<EnvioDetalhadoDto>> ObterEnvioAsync(int id)
    {
        var envio = await _envioRepo.ObterDetalhadoPorIdAsync(id);
        if (envio is null)
        return ApiResponseDto<EnvioDetalhadoDto>.Erro("Envio não encontrado");

    return ApiResponseDto<EnvioDetalhadoDto>.Ok(envio);
    }

    public async Task<ApiResponseDto<PagedResultDto<EnvioResumoDto>>> ListarEnviosAsync(FiltroEnvioDto filtro)
    {
        var resultado = await _envioRepo.ListarAsync(filtro);
        return ApiResponseDto<PagedResultDto<EnvioResumoDto>>.Ok(resultado);
    }

    public async Task<ApiResponseDto<EnvioDetalhadoDto>> AvaliarEnvioAsync(int id, AvaliarEnvioDto dto)
    {
        var envio = await _envioRepo.ObterPorIdAsync(id);
        if(envio is null)
        return ApiResponseDto<EnvioDetalhadoDto>.Erro("Envio não encontrado.");

        if(envio.IdStatusEnvio != STATUS_PENDENTE)
        return ApiResponseDto<EnvioDetalhadoDto>.Erro("Apenas envios com status pendentes podem ser avaliados.");

        if(dto.IdStatusEnvio != STATUS_CONCLUIDO && dto.IdStatusEnvio != STATUS_CANCELADO)
        return ApiResponseDto<EnvioDetalhadoDto>.Erro("Status inválido. Use 2 (Concluído) ou 3 (Cancelado).");

        await _envioRepo.AtualizarStatusAsync(id, dto.IdStatusEnvio, dto.Observacao);

        //Se concluido, notifica a API de Pontuação para calcular e regiatrar os pontos

        if (dto.IdStatusEnvio == STATUS_CONCLUIDO)
        await NotificarApiPontuacaoAsync(id);

        var detalhe = await _envioRepo.ObterDetalhadoPorIdAsync(id);
        return ApiResponseDto<EnvioDetalhadoDto>.Ok(detalhe!, "Envio avaliado com sucesso.");
    }

    public async Task<ApiResponseDto<bool>> CancelarEnvioAsync(int id)
    {
        var envio = await _envioRepo.ObterPorIdAsync(id);
        if (envio is null)
        return ApiResponseDto<bool>.Erro("Envio não encontrado.");

        if (envio.IdStatusEnvio != STATUS_PENDENTE)
        return ApiResponseDto<bool>.Erro("Somente envios pendentes podem ser cancelados.");

        await _envioRepo.AtualizarStatusAsync(id, STATUS_CANCELADO, "Cancelado pelo usuário");
        return ApiResponseDto<bool>.Ok(true, "Envio cancelado com sucesso.");
    }

    ///------ Integração com a API de Pontuação ------
    
    private async Task NotificarApiPontuacaoAsync(int idEnvio)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiPontuacao");
            var response = await client.PostAsJsonAsync(
                "/api/pontuacao/calcular",
                new { idEnvio = idEnvio });

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning(
                    "API de Pontuação retornou erro {Status} para o envio {IdEnvio}.",
                    response.StatusCode, idEnvio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao notificar API de Pontuação para o envio {IdEnvio}.", idEnvio);
        }
    }

    public async Task<ApiResponseDto<bool>> ExcluirEnvioAsync(int id)
    {
        var existe = await _envioRepo.ExisteAsync(id);
        if(!existe)
        return ApiResponseDto<bool>.Erro("Envio não encontrado.");

        await _envioRepo.ExcluirAsync(id);
        return ApiResponseDto<bool>.Ok(true, "Envio excluído com sucesso.");
    }

    public async Task<ApiResponseDto<EnvioDetalhadoDto>> EditarEnvioAsync(int id, EditarEnvioDto dto)
    {
        var existe = await _envioRepo.ExisteAsync(id);
        if (!existe)
        return ApiResponseDto<EnvioDetalhadoDto>.Erro("Envio não encontrado.");

        await _envioRepo.EditarAsync(id, dto);
        var detalhe = await _envioRepo.ObterDetalhadoPorIdAsync(id);
        return ApiResponseDto<EnvioDetalhadoDto>.Ok(detalhe!, "Envio atualizado com sucesso!");
    }

}