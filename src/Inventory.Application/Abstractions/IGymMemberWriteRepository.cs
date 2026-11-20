using Inventory.Domain.Entities;
namespace Inventory.Application.Abstractions;
public interface IGymMemberWriteRepository
{
    Task CreateAsync(GymMember member, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(GymMember member, CancellationToken cancellationToken);
}
