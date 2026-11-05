using System.Data.Common;
using Dapper;
using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using Inventory.Domain.Rules;
namespace Inventory.Infrastructure.Persistence.Dapper;
public sealed class InventoryWriteRepository : IInventoryWriteRepository
{
    private const string SelectStockSql = "SELECT Stock FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = @ProductId AND IsActive = 1;";
    private const string InsertSql = @"INSERT INTO InventoryMovements (Id, ProductId, Type, Quantity, UnitCost, Reason, Reference, CreatedBy, CreatedAt)
VALUES (@Id, @ProductId, @Type, @Quantity, @UnitCost, @Reason, @Reference, @CreatedBy, @CreatedAt);";
    private const string UpdateStockSql = "UPDATE Products SET Stock = @Stock, UpdatedAt = @UpdatedAt WHERE Id = @ProductId;";
    private readonly IDbConnectionFactory _connectionFactory;
    public InventoryWriteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    public async Task<MovementRegistrationResult> RegisterAsync(InventoryMovement movement, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        var currentStock = await GetStockForUpdateAsync(connection, movement.ProductId, transaction, cancellationToken);
        if (currentStock is null)
        {
            return new MovementRegistrationResult(MovementRegistrationStatus.ProductNotFound, 0);
        }
        var result = BuildResult(currentStock.Value, movement);
        if (result.Status != MovementRegistrationStatus.Created)
        {
            return result;
        }
        await InsertMovementAsync(connection, movement, transaction, cancellationToken);
        await UpdateStockAsync(connection, movement.ProductId, result.CurrentStock, movement.CreatedAt, transaction, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }
    private static MovementRegistrationResult BuildResult(int currentStock, InventoryMovement movement)
    {
        try
        {
            var nextStock = StockPolicy.Apply(currentStock, movement.Type, movement.Quantity);
            return new MovementRegistrationResult(MovementRegistrationStatus.Created, nextStock);
        }
        catch (InsufficientStockException)
        {
            return new MovementRegistrationResult(MovementRegistrationStatus.InsufficientStock, currentStock);
        }
    }
    private static async Task<int?> GetStockForUpdateAsync(DbConnection connection, Guid productId, DbTransaction transaction, CancellationToken cancellationToken)
    {
        return await connection.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(SelectStockSql, new { ProductId = productId }, transaction, cancellationToken: cancellationToken));
    }
    private static Task InsertMovementAsync(DbConnection connection, InventoryMovement movement, DbTransaction transaction, CancellationToken cancellationToken)
    {
        return connection.ExecuteAsync(new CommandDefinition(InsertSql, movement, transaction, cancellationToken: cancellationToken));
    }
    private static Task UpdateStockAsync(DbConnection connection, Guid productId, int stock, DateTime utcNow, DbTransaction transaction, CancellationToken cancellationToken)
    {
        return connection.ExecuteAsync(new CommandDefinition(UpdateStockSql, new { ProductId = productId, Stock = stock, UpdatedAt = utcNow }, transaction, cancellationToken: cancellationToken));
    }
}
