using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TechChallenge.Domain.Entities.Base;
using TechChallenge.Domain.Interface.BaseRepository;
using static Dapper.SqlMapper;

namespace TechChallenge.Data.Base;
public class CrudRepository<T>(IConfiguration configuration) : ICrudRepository<T> where T : IEntity
{
    protected readonly string _connString = configuration["ConnectionStrings:ConnString"]!;

    public async Task<T?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var tableName = typeof(CrudRepository<T>).GetMethod("GetById")!.ReturnType.GenericTypeArguments.First().Name;

        var sql = $"SELECT * FROM dbo.{tableName} WITH (NOLOCK) WHERE Id = @id ";

        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync(cancellationToken);
        return await conn.QueryFirstOrDefaultAsync<T>(sql, param: new { id });
    }

    public async Task<List<T>> RawQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync(cancellationToken);
        return (await conn.QueryAsync<T>(sql, cancellationToken)).ToList();
    }

    public async Task<T?> RawQueryFirstOrDefaultAsync(string sql, CancellationToken cancellationToken = default)
    {
        using var conn = new SqlConnection(_connString);
        await conn.OpenAsync(cancellationToken);
        return await conn.QueryFirstOrDefaultAsync<T?>(sql, cancellationToken);
    }
}
