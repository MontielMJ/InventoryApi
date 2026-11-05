using Dapper;
using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
namespace Inventory.Infrastructure.Persistence.Dapper;
public sealed class CategoryWriteRepository : ICategoryWriteRepository
{
    private const string InsertSql = @"INSERT INTO Categories (Id, Name, Description, IsActive, CreatedAt, UpdatedAt)
VALUES (@Id, @Name, @Description, @IsActive, @CreatedAt, @UpdatedAt);";
    private const string UpdateSql = "UPDATE Categories SET Name = @Name, Description = @Description, UpdatedAt = @UpdatedAt WHERE Id = @Id AND IsActive = 1;";
    private const string DeactivateSql = "UPDATE Categories SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id AND IsActive = 1;";
    private readonly IDbConnectionFactory _connectionFactory;
    public CategoryWriteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    public async Task CreateAsync(Category category, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(InsertSql, category, cancellationToken: cancellationToken));
    }
    public async Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(UpdateSql, category, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
    public async Task<bool> DeactivateAsync(Guid id, DateTime utcNow, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(DeactivateSql, new { Id = id, UpdatedAt = utcNow }, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
}
