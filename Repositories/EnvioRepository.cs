using ApiEnvio.DTOs;
using ApiEnvio.Models;
using ApiEnvio.Repositories.Interfaces;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace ApiEnvio.Repositories;

public class EnvioRepository : IEnvioRepository
{
    private readonly string _connectionString;

    public EnvioRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    private IDbConnection CriarConexao() => new MySqlConnection(_connectionString);

    public async Task<int> CriarAsync(Envio envio)
    {
        using var conn = CriarConexao();
        const string sql = @"
        INSERT INTO envio (id_usuario, id_empresa, data_envio, id_status_envio, observacao, quantidade_itens)
        VALUES (@IdUsuario, @IdEmpresa, @DataEnvio, @IdStatusEnvio, @Observacao, @QuantidadeItens);
        SELECT LAST_INSERT_ID();
        ";

        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            envio.IdUsuario,
            envio.IdEmpresa,
            envio.DataEnvio,
            envio.IdStatusEnvio,
            envio.Observacao,
            envio.QuantidadeItens
        });
    }

    public async Task<Envio?> ObterPorIdAsync(int id)
    {
        using var conn = CriarConexao();
        const string sql = @"
            SELECT id Id, id_usuario IdUsuario, id_empresa IdEmpresa,
                   data_envio DataEnvio, id_status_envio IdStatusEnvio,
                   observacao Observacao, quantidade_itens QuantidadeItens
            FROM envio WHERE id = @Id";
 
        return await conn.QueryFirstOrDefaultAsync<Envio>(sql, new { Id = id });
    }

    public async Task<EnvioDetalhadoDto?> ObterDetalhadoPorIdAsync(int id)
    {
        using var conn = CriarConexao();
 
        const string sqlEnvio = @"
            SELECT
                e.id                Id,
                e.id_usuario        IdUsuario,
                u.nome              NomeUsuario,
                e.id_empresa        IdEmpresa,
                emp.nome            NomeEmpresa,
                e.data_envio        DataEnvio,
                se.nome_status      StatusEnvio,
                e.id_status_envio   IdStatusEnvio,
                e.observacao        Observacao,
                e.quantidade_itens  QuantidadeItens
            FROM envio e
            INNER JOIN usuario      u   ON u.id   = e.id_usuario
            INNER JOIN empresa      emp ON emp.id = e.id_empresa
            INNER JOIN status_envio se  ON se.id  = e.id_status_envio
            WHERE e.id = @Id";
 
        var envio = await conn.QueryFirstOrDefaultAsync<EnvioDetalhadoDto>(sqlEnvio, new { Id = id });
        if (envio is null) return null;
 
        const string sqlItens = @"
            SELECT
                ie.id           Id,
                ie.id_embalagem IdEmbalagem,
                me.id           IdMaterial,
                emb.descricao   DescricaoEmbalagem,
                me.nome         MaterialEmbalagem,
                emb.peso_medio  PesoMedio
            FROM item_envio ie
            INNER JOIN embalagem          emb ON emb.id = ie.id_embalagem
            INNER JOIN material_embalagem me  ON me.id  = emb.id_tipo
            WHERE ie.id_envio = @IdEnvio";
 
        envio.Itens = (await conn.QueryAsync<ItemEnvioDto>(sqlItens, new { IdEnvio = id })).ToList();
 
        return envio;
    }
 
    public async Task<PagedResultDto<EnvioResumoDto>> ListarAsync(FiltroEnvioDto filtro)
    {
        using var conn = CriarConexao();
 
        var where = new List<string>();
        var parametros = new DynamicParameters();
 
        if (filtro.IdUsuario.HasValue)
        {
            where.Add("e.id_usuario = @IdUsuario");
            parametros.Add("IdUsuario", filtro.IdUsuario.Value);
        }
        if (filtro.IdEmpresa.HasValue)
        {
            where.Add("e.id_empresa = @IdEmpresa");
            parametros.Add("IdEmpresa", filtro.IdEmpresa.Value);
        }
        if (filtro.IdStatus.HasValue)
        {
            where.Add("e.id_status_envio = @IdStatus");
            parametros.Add("IdStatus", filtro.IdStatus.Value);
        }
        if (filtro.DataInicio.HasValue)
        {
            where.Add("e.data_envio >= @DataInicio");
            parametros.Add("DataInicio", filtro.DataInicio.Value);
        }
        if (filtro.DataFim.HasValue)
        {
            where.Add("e.data_envio <= @DataFim");
            parametros.Add("DataFim", filtro.DataFim.Value.Date.AddDays(1).AddSeconds(-1));
        }
 
        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
 
        var total = await conn.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM envio e {whereClause}", parametros);
 
        parametros.Add("Offset", (filtro.Pagina - 1) * filtro.TamanhoPagina);
        parametros.Add("Limit", filtro.TamanhoPagina);
 
        var sqlLista = $@"
            SELECT
                e.id                Id,
                u.nome              NomeUsuario,
                emp.nome            NomeEmpresa,
                e.data_envio        DataEnvio,
                se.nome_status      StatusEnvio,
                e.quantidade_itens  QuantidadeItens
            FROM envio e
            INNER JOIN usuario      u   ON u.id   = e.id_usuario
            INNER JOIN empresa      emp ON emp.id = e.id_empresa
            INNER JOIN status_envio se  ON se.id  = e.id_status_envio
            {whereClause}
            ORDER BY e.data_envio DESC
            LIMIT @Limit OFFSET @Offset";
 
        var dados = (await conn.QueryAsync<EnvioResumoDto>(sqlLista, parametros)).ToList();
 
        return new PagedResultDto<EnvioResumoDto>
        {
            Dados = dados,
            Total = total,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        };
    }
 
    public async Task AtualizarStatusAsync(int id, int idStatus, string? observacao)
    {
        using var conn = CriarConexao();
        const string sql = @"
            UPDATE envio SET id_status_envio = @IdStatus, observacao = @Observacao
            WHERE id = @Id";
 
        await conn.ExecuteAsync(sql, new { Id = id, IdStatus = idStatus, Observacao = observacao });
    }
 
    public async Task<bool> ExisteAsync(int id)
    {
        using var conn = CriarConexao();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM envio WHERE id = @Id", new { Id = id });
        return count > 0;
    }

    public async Task ExcluirAsync(int id)
    {
        using var conn = CriarConexao();
        await conn.ExecuteAsync("DELETE FROM envio WHERE id = @Id", new {Id = id});
    }

    public async Task EditarAsync(int id, EditarEnvioDto dto)
    {
        using var conn = CriarConexao();
        const string sql = @"
        UPDATE envio SET
        data_envio = @DataEnvio,
        id_empresa = @IdEmpresa,
        quantidade_itens = @QuantidadeItens
        WHERE id = @Id";

        await conn.ExecuteAsync(sql, new {Id = id, dto.DataEnvio, dto.IdEmpresa, dto.QuantidadeItens});
    }

}