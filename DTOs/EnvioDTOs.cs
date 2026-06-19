using System.ComponentModel.DataAnnotations;
 
namespace ApiEnvio.DTOs;
 
// ───── REQUEST ─────
 
public class CriarEnvioDto
{
    [Required(ErrorMessage = "O ID do usuário é obrigatório.")]
    public int IdUsuario { get; set; }
 
    [Required(ErrorMessage = "O ID da empresa é obrigatório.")]
    public int IdEmpresa { get; set; }
 
    public string? Observacao { get; set; }
 
    [Required(ErrorMessage = "Informe ao menos uma embalagem.")]
    [MinLength(1, ErrorMessage = "Informe ao menos uma embalagem.")]
    public List<int> IdsEmbalagens { get; set; } = new();
}
 
public class AvaliarEnvioDto
{
    [Required(ErrorMessage = "O status é obrigatório.")]
    public int IdStatusEnvio { get; set; } // 2 = Concluído, 3 = Cancelado
 
    public string? Observacao { get; set; }
}
 
public class FiltroEnvioDto
{
    public int? IdUsuario { get; set; }
    public int? IdEmpresa { get; set; }
    public int? IdStatus { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 1000;
}
 
// ───── RESPONSE ─────
 
public class EnvioDetalhadoDto
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public int IdEmpresa { get; set; }
    public string NomeEmpresa { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }
    public string StatusEnvio { get; set; } = string.Empty;
    public int IdStatusEnvio { get; set; }
    public string? Observacao { get; set; }
    public int QuantidadeItens { get; set; }
    public List<ItemEnvioDto> Itens { get; set; } = new();
}
 
public class ItemEnvioDto
{
    public int Id { get; set; }
    public int IdEmbalagem { get; set; }
    public int IdMaterial {get; set;}
    public string DescricaoEmbalagem { get; set; } = string.Empty;
    public string MaterialEmbalagem { get; set; } = string.Empty;
    public decimal PesoMedio { get; set; }
}
 
public class EnvioResumoDto
{
    public int Id { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string NomeEmpresa { get; set; } = string.Empty;
    public DateTime DataEnvio { get; set; }
    public string StatusEnvio { get; set; } = string.Empty;
    public int QuantidadeItens { get; set; }
}
 
public class PagedResultDto<T>
{
    public List<T> Dados { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanhoPagina);
}
 
public class ApiResponseDto<T>
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public T? Dados { get; set; }
 
    public static ApiResponseDto<T> Ok(T dados, string mensagem = "Operação realizada com sucesso.")
        => new() { Sucesso = true, Mensagem = mensagem, Dados = dados };
 
    public static ApiResponseDto<T> Erro(string mensagem)
        => new() { Sucesso = false, Mensagem = mensagem };
}

public class EditarEnvioDto
{
    [Required]
    public DateTime DataEnvio { get; set; }

    [Required]
    public int IdEmpresa { get; set; }

    [Required]
    public int QuantidadeItens { get; set; }
}
