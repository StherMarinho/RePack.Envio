using ApiEnvio.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
 
namespace ApiEnvio.Repositories;
 
public class ItemEnvioRepository : IItemEnvioRepository
{
    private readonly string _connectionString;
 
    public ItemEnvioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
 
    private IDbConnection CriarConexao() => new MySqlConnection(_connectionString);
 
    public async Task InserirItensAsync(int idEnvio, IEnumerable<int> idsEmbalagens)
    {
        using var conn = CriarConexao();
        const string sql = @"
            INSERT INTO item_envio (id_envio, id_embalagem)
            VALUES (@IdEnvio, @IdEmbalagem)";
 
        var itens = idsEmbalagens.Select(id => new { IdEnvio = idEnvio, IdEmbalagem = id });
        await conn.ExecuteAsync(sql, itens);
    }
 
    public async Task<bool> EmbalagemExisteEAtivaAsync(int idEmbalagem)
    {
        using var conn = CriarConexao();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM embalagem WHERE id = @Id AND ativo = 1",
            new { Id = idEmbalagem });
        return count > 0;
    }
}