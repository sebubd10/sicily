using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

/// <summary>
/// Tracks a customer's reward points balance.
/// StoreId = null when PointsAccumulatedForAllStores = true.
/// </summary>
public class RewardPointsAccount : TenantEntity
{
    public Guid CustomerId { get; private set; }
    public Guid? StoreId { get; private set; }

    public int TotalEarnedPoints { get; private set; }
    public int UsedPoints { get; private set; }
    public int ExpiredPoints { get; private set; }
    public int PendingPoints { get; private set; }

    public int AvailablePoints => TotalEarnedPoints - UsedPoints - ExpiredPoints;

    private readonly List<RewardPointsEntry> _entries = [];
    public IReadOnlyCollection<RewardPointsEntry> Entries => _entries.AsReadOnly();

    private RewardPointsAccount() { }

    public static RewardPointsAccount Create(Guid tenantId, Guid customerId, Guid? storeId = null) =>
        new() { TenantId = tenantId, CustomerId = customerId, StoreId = storeId };

    public RewardPointsEntry EarnPoints(int points, RewardPointsEntryType type,
        bool activateImmediately, int validityDays,
        Guid? transactionId = null, string? notes = null)
    {
        if (points <= 0) throw new DomainException("Points must be greater than zero.");

        DateTime? expiresAt = validityDays > 0
            ? DateTime.UtcNow.AddDays(validityDays) : null;

        var entry = RewardPointsEntry.Create(Id, points, type, activateImmediately, expiresAt,
            transactionId, notes);
        _entries.Add(entry);

        if (activateImmediately)
            TotalEarnedPoints += points;
        else
            PendingPoints += points;

        UpdatedAt = DateTime.UtcNow;
        return entry;
    }

    public RewardPointsEntry RedeemPoints(int points, Guid? transactionId = null, string? notes = null)
    {
        if (points <= 0) throw new DomainException("Points to redeem must be greater than zero.");
        if (points > AvailablePoints)
            throw new DomainException($"Insufficient reward points. Available: {AvailablePoints}.");

        var entry = RewardPointsEntry.Create(Id, -points, RewardPointsEntryType.Redeemed,
            true, null, transactionId, notes);
        _entries.Add(entry);
        UsedPoints += points;
        UpdatedAt = DateTime.UtcNow;
        return entry;
    }

    public void ActivatePendingPoints(int points)
    {
        var toActivate = Math.Min(points, PendingPoints);
        if (toActivate <= 0) return;
        PendingPoints -= toActivate;
        TotalEarnedPoints += toActivate;
        UpdatedAt = DateTime.UtcNow;
    }

    public RewardPointsEntry ManualAdjustment(int points, string notes)
    {
        var entry = RewardPointsEntry.Create(Id, points, RewardPointsEntryType.ManualAdjustment,
            true, null, null, notes);
        _entries.Add(entry);
        if (points > 0) TotalEarnedPoints += points;
        else UsedPoints += Math.Abs(points);
        UpdatedAt = DateTime.UtcNow;
        return entry;
    }
}

public class RewardPointsEntry : BaseEntity
{
    public Guid RewardPointsAccountId { get; private set; }
    public int Points { get; private set; }
    public RewardPointsEntryType EntryType { get; private set; }
    public bool IsActivated { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public Guid? TransactionId { get; private set; }
    public string? Notes { get; private set; }

    private RewardPointsEntry() { }

    internal static RewardPointsEntry Create(Guid accountId, int points,
        RewardPointsEntryType type, bool activated, DateTime? expiresAt,
        Guid? transactionId, string? notes) =>
        new()
        {
            RewardPointsAccountId = accountId,
            Points = points,
            EntryType = type,
            IsActivated = activated,
            ExpiresAt = expiresAt,
            TransactionId = transactionId,
            Notes = notes
        };
}
