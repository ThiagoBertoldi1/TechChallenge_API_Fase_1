namespace TechChallenge.Domain.Interface.BaseRepository.Queue;
public interface IQueueRepository<T>
{
    Task<bool> InsertAsync(T entity, CancellationToken cancellationToken = new());
    Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = new());
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}
