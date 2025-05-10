namespace TechChallenge.Domain.Interface.BaseRepository;
public interface ICrudRepository<T>
{
    Task<T?> GetById(Guid id, CancellationToken cancellationToken = new());
    protected Task<T?> RawQueryFirstOrDefaultAsync(string sql, CancellationToken cancellationToken = default);
    protected Task<List<T>> RawQueryAsync(string sql, CancellationToken cancellationToken = default);
}
