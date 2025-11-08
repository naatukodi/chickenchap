// Infrastructure/ICosmosRepository.cs
namespace ChickenChap.Api.Infrastructure;

public interface ICosmosRepository<T>
{
    Task<T> CreateAsync(T entity, string pk);
    Task<T?> GetAsync(string id, string pk);
    IAsyncEnumerable<T> QueryAsync(string query, params (string Name, object Value)[] parameters);
    Task<T> UpsertAsync(T entity, string pk);
    Task DeleteAsync(string id, string pk);
}
