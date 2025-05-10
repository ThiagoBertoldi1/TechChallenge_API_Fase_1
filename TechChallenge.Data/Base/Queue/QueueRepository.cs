using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TechChallenge.Domain.Entities.Base;
using TechChallenge.Domain.Interface.BaseRepository.Queue;

namespace TechChallenge.Data.Base.Queue;
public class QueueRepository<T>(IConfiguration configuration) : IQueueRepository<T> where T : IEntity
{
    protected readonly string _connString = configuration["ConnectionStrings:ConnString"]!;

    public async Task<bool> InsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        var tableName = entity!.GetType().Name;
        var properties = entity.GetType().GetProperties();

        var columnNames = string.Join(", ", properties.Select(p => p.Name));
        var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var insertSql = $"INSERT INTO dbo.{tableName} ({columnNames}) VALUES ({parameterNames}) ";

        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync(cancellationToken);
        return await conn.ExecuteAsync(insertSql, param: entity) == 1;
    }

    public async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var tableName = entity!.GetType().Name;
        var properties = entity.GetType().GetProperties();

        var keyProperty = properties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
            ?? throw new Exception("Chave primária 'Id' não encontrada.");

        var setClause = string.Join(", ", properties
            .Where(p => p.Name != keyProperty.Name)
            .Select(p => $"{p.Name} = @{p.Name}"));

        var updateSql = $"UPDATE dbo.{tableName} SET {setClause} WHERE {keyProperty.Name} = @{keyProperty.Name}";

        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync(cancellationToken);
        return await conn.ExecuteAsync(updateSql, param: entity) == 1;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tableName = typeof(CrudRepository<T>).GetMethod("GetById")!.ReturnType.GenericTypeArguments.First().Name;

        var sql = $"DELETE FROM dbo.{tableName} WHERE Id = @id";

        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync(cancellationToken);
        return await conn.ExecuteAsync(sql, param: new { id }) == 1;
    }
}
