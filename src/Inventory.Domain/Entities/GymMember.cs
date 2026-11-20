using Inventory.Domain.Enums;
namespace Inventory.Domain.Entities;
public sealed class GymMember
{
    private GymMember() { }
    public Guid Id { get; private set; }
    public string MemberNumber { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public GymMemberStatus Status { get; private set; }
    public DateTime MembershipStartDate { get; private set; }
    public DateTime? MembershipEndDate { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public static GymMember Create(string memberNumber, string fullName, string email, string phone, DateTime membershipStartDate, DateTime utcNow)
    {
        return new GymMember
        {
            Id = Guid.NewGuid(),
            MemberNumber = memberNumber.Trim().ToUpperInvariant(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone.Trim(),
            Status = GymMemberStatus.Active,
            MembershipStartDate = membershipStartDate.Date,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };
    }
    public void Update(string memberNumber, string fullName, string email, string phone, GymMemberStatus status, DateTime membershipStartDate, DateTime? membershipEndDate, string? cancellationReason, DateTime utcNow)
    {
        MemberNumber = memberNumber.Trim().ToUpperInvariant();
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Status = status;
        MembershipStartDate = membershipStartDate.Date;
        MembershipEndDate = membershipEndDate?.Date;
        CancellationReason = cancellationReason?.Trim();
        UpdatedAt = utcNow;
    }
}
