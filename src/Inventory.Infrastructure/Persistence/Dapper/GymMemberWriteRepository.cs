using Dapper;
using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
namespace Inventory.Infrastructure.Persistence.Dapper;
public sealed class GymMemberWriteRepository : IGymMemberWriteRepository
{
    private const string InsertSql = "INSERT INTO GymMembers (Id, MemberNumber, FullName, Email, Phone, Status, MembershipStartDate, MembershipEndDate, CancellationReason, CreatedAt, UpdatedAt) VALUES (@Id, @MemberNumber, @FullName, @Email, @Phone, @Status, @MembershipStartDate, @MembershipEndDate, @CancellationReason, @CreatedAt, @UpdatedAt);";
    private const string UpdateSql = "UPDATE GymMembers SET MemberNumber = @MemberNumber, FullName = @FullName, Email = @Email, Phone = @Phone, Status = @Status, MembershipStartDate = @MembershipStartDate, MembershipEndDate = @MembershipEndDate, CancellationReason = @CancellationReason, UpdatedAt = @UpdatedAt WHERE Id = @Id;";
    private readonly IDbConnectionFactory _connectionFactory;
    public GymMemberWriteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    public async Task CreateAsync(GymMember member, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(InsertSql, member, cancellationToken: cancellationToken));
    }
    public async Task<bool> UpdateAsync(GymMember member, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(new CommandDefinition(UpdateSql, member, cancellationToken: cancellationToken));
        return affectedRows > 0;
    }
}
