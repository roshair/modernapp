namespace MyModernApp.Payments
{
    public class SettlementEngine
    {
        public static readonly TimeSpan InternationalSettlementSla = TimeSpan.FromMinutes(10);

        public static bool IsDelayedInternationalFxPending(PaymentTransaction transaction, DateTime nowUtc)
        {
            return transaction.IsInternational
                && transaction.Route == TransferRoute.FxConversionService
                && transaction.Status == SettlementStatus.PendingSettlement
                && nowUtc - transaction.InitiatedAtUtc > InternationalSettlementSla;
        }

        public void ExpediteDelayedInternationalTransfers(IEnumerable<PaymentTransaction> transactions, DateTime nowUtc)
        {
            foreach (var transaction in transactions)
            {
                if (!IsDelayedInternationalFxPending(transaction, nowUtc))
                {
                    continue;
                }

                transaction.MarkSettled(nowUtc);
            }
        }
    }
}
