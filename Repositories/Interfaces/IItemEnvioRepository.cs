namespace ApiEnvio.Repositories.Interfaces;
 
public interface IItemEnvioRepository
{
    Task InserirItensAsync(int idEnvio, IEnumerable<int> idsEmbalagens);
    Task<bool> EmbalagemExisteEAtivaAsync(int idEmbalagem);
}