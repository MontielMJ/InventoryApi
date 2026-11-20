using Inventory.Application.Abstractions;
using Inventory.Domain.Entities;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.GymMembers.Commands.CreateGymMember;
public sealed class CreateGymMemberCommandHandler : IRequestHandler<CreateGymMemberCommand, Guid>
{
    private readonly IGymMemberReadRepository _readRepository;
    private readonly IGymMemberWriteRepository _writeRepository;
    private readonly TimeProvider _timeProvider;
    public CreateGymMemberCommandHandler(IGymMemberReadRepository readRepository, IGymMemberWriteRepository writeRepository, TimeProvider timeProvider)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _timeProvider = timeProvider;
    }
    public async Task<Guid> Handle(CreateGymMemberCommand request, CancellationToken cancellationToken)
    {
        if (await _readRepository.MemberNumberExistsAsync(request.MemberNumber, null, cancellationToken))
        {
            throw new ConflictException($"A gym member with number '{request.MemberNumber}' already exists.");
        }
        var member = GymMember.Create(request.MemberNumber, request.FullName, request.Email, request.Phone, request.MembershipStartDate, _timeProvider.GetUtcNow().UtcDateTime);
        await _writeRepository.CreateAsync(member, cancellationToken);
        return member.Id;
    }
}
