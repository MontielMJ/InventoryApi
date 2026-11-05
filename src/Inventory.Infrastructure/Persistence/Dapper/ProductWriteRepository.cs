using Dapper;
using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
namespace Inventory.Infrastructure.Persistence.Dapper;
public sealed class ProductWriteRepository : IProductWriteRepository
{
    private const string InsertSql = @"INSERT INTO Products (Id, Sku, Name, Description, CategoryId, Price, Stock, MinimumStock, IsActive, CreatedAt, UpdatedAt)
VALUES (@Id, @Sku, @Name, @Description, @CategoryId, @Price, @Stock, @MinimumStock, @IsActive, @CreatedAt, @UpdatedAt);";
    private const string UpdateSql = @"UPDATE Products SET Sku = @Sku, Name = @Name, Description = @Description, CategoryId = @CategoryId,
Price = @Price, MinimumStock = @MinimumStock, UpdatedAt = @UpdatedAt
WHERE Id = @Id AND IsActive = 1;";
    private const string DeactivateSql = "UPDATE Products SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id AND IsActive = 1;";
    private readonly IDbConnectionFactory _connectionFactory;
    public ProductWriteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    public async Task CreateAsync(Product product, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(InsertSql, product, cancellationToken: cancellationToken));
    }
    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(UpdateSql, product, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
    public async Task<bool> DeactivateAsync(Guid id, DateTime utcNow, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(DeactivateSql, new { Id = id, UpdatedAt = utcNow }, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
}
