namespace MyModernApp.Payments
{
    public class PaymentTransaction
    {
        public required string Id { get; init; }

        public required bool IsInternational { get; init; }

        public required TransferRoute Route { get; init; }

        public required DateTime InitiatedAtUtc { get; init; }

        public DateTime? SettledAtUtc { get; private set; }

        public SettlementStatus Status { get; private set; } = SettlementStatus.PendingSettlement;

        public void MarkSettled(DateTime settledAtUtc)
        {
            Status = SettlementStatus.Settled;
            SettledAtUtc = settledAtUtc;
        }
    }
}
