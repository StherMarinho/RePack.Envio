namespace ApiEnvio.Models;
 
public class Envio
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public int IdEmpresa { get; set; }
    public DateTime DataEnvio { get; set; }
    public int IdStatusEnvio { get; set; }
    public string? Observacao { get; set; }
    public int QuantidadeItens { get; set; }
}
 
public class ItemEnvio
{
    public int Id { get; set; }
    public int IdEnvio { get; set; }
    public int IdEmbalagem { get; set; }
}