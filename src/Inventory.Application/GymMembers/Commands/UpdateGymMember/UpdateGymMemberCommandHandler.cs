using Inventory.Application.Abstractions;
using Inventory.Domain.Exceptions;
using MediatR;
namespace Inventory.Application.GymMembers.Commands.UpdateGymMember;
public sealed class UpdateGymMemberCommandHandler : IRequestHandler<UpdateGymMemberCommand>
{
    private readonly IGymMemberReadRepository _readRepository;
    private readonly IGymMemberWriteRepository _writeRepository;
    private readonly TimeProvider _timeProvider;
    public UpdateGymMemberCommandHandler(IGymMemberReadRepository readRepository, IGymMemberWriteRepository writeRepository, TimeProvider timeProvider)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _timeProvider = timeProvider;
    }
    public async Task Handle(UpdateGymMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _readRepository.GetEntityAsync(request.Id, cancellationToken) ?? throw new NotFoundException($"Gym member '{request.Id}' was not found.");
        if (await _readRepository.MemberNumberExistsAsync(request.MemberNumber, request.Id, cancellationToken))
        {
            throw new ConflictException($"A gym member with number '{request.MemberNumber}' already exists.");
        }
        member.Update(request.MemberNumber, request.FullName, request.Email, request.Phone, request.Status, request.MembershipStartDate, request.MembershipEndDate, request.CancellationReason, _timeProvider.GetUtcNow().UtcDateTime);
        if (!await _writeRepository.UpdateAsync(member, cancellationToken))
        {
            throw new NotFoundException($"Gym member '{request.Id}' was not found.");
        }
    }
}
