using ApiEnvio.DTOs;
using ApiEnvio.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiEnvio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnvioController : ControllerBase
{
    private readonly IEnvioService _service;

    public EnvioController(IEnvioService service)
    {
        _service = service;
    }

    /// <summary>
    /// Registra um novo envio com o status pendente.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<EnvioDetalhadoDto>), 201)]
    [ProducesResponseType(typeof(ApiResponseDto<EnvioDetalhadoDto>), 400)]
    public async Task<IActionResult> Criar([FromBody] CriarEnvioDto dto)
    {
        if (!ModelState.IsValid)
        return BadRequest(ApiResponseDto<EnvioDetalhadoDto>.Erro("Dados inválidos."));
    

    var resultado = await _service.CriarEnvioAsync(dto);

    return resultado.Sucesso
            ? CreatedAtAction(nameof(ObterPorId), new { id = resultado.Dados!.Id }, resultado)
            : BadRequest(resultado);

    }

    /// <summary>
    /// Retorna os detalhes de um envio pelo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<EnvioDetalhadoDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _service.ObterEnvioAsync(id);

        return resultado.Sucesso ? Ok (resultado) : NotFound (resultado);
    }

    /// <summary>
    /// Lista envios com filtros opcionais e paginação
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResultDto<EnvioResumoDto>>), 200)]
    public async Task<IActionResult> Listar([FromQuery] FiltroEnvioDto filtro)
    {
        var resultado = await _service.ListarEnviosAsync(filtro);
        return Ok (resultado);
    }

    /// <summary>
    /// Avalia um envio pendente (Concuido = 2 ou Cancelado = 3).
    /// Ao concluir, a Api de Pontuação é notificada automaticamente.
    /// </summary>
    [HttpPatch("{id:int}/avaliar")]
    [ProducesResponseType(typeof(ApiResponseDto<EnvioDetalhadoDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<EnvioDetalhadoDto>), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Avaliar(int id, [FromBody] AvaliarEnvioDto dto)
    {
        if(!ModelState.IsValid)
        return BadRequest(ApiResponseDto<EnvioDetalhadoDto>.Erro("Dados inválidos."));

        var resultado = await _service.AvaliarEnvioAsync(id, dto);

        if(!resultado.Sucesso)
        return resultado.Mensagem.Contains("não encontrado")
        ? NotFound (resultado)
        : BadRequest(resultado);

        return Ok(resultado);
    }
    
    /// <summary>
    /// Cancela um envio que ainda está com status Pendente.
    /// </summary>
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancelar(int id)
    {
        var resultado = await _service.CancelarEnvioAsync(id);

        if(!resultado.Sucesso)
        return resultado.Mensagem.Contains("não encontrado")
        ? NotFound(resultado)
        : BadRequest(resultado);

    return Ok(resultado);
    }

    /// <summary>
    /// Exclui um envio. Exclusivo para administrdores.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _service.ExcluirEnvioAsync(id);

        if(!resultado.Sucesso)
        return resultado.Mensagem.Contains("não encontrado") ? NotFound(resultado) : BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>
    /// Edita data, empresa e quantidade de itens de um envio. Exclusivo para administradores.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponseDto<EnvioDetalhadoDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarEnvioDto dto)
    {
        if(!ModelState.IsValid)
        return BadRequest(ApiResponseDto<EnvioDetalhadoDto>.Erro("Dados inválidos"));

        var resultado = await _service.EditarEnvioAsync(id, dto);

        if(!resultado.Sucesso)
        return resultado.Mensagem.Contains("não encontrado") ? NotFound(resultado) : BadRequest(resultado);

        return Ok(resultado);
    }

}