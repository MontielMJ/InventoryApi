namespace Inventory.Application.Abstractions;
public sealed record MovementRegistrationResult(MovementRegistrationStatus Status, int CurrentStock);
