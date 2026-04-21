namespace MyModernApp.Payments
{
    public class SettlementEngine
    {
        private static readonly TimeSpan InternationalSettlementSla = TimeSpan.FromMinutes(10);

        public void ExpediteDelayedInternationalTransfers(IEnumerable<PaymentTransaction> transactions, DateTime nowUtc)
        {
            foreach (var transaction in transactions)
            {
                if (transaction.Status != SettlementStatus.PendingSettlement)
                {
                    continue;
                }

                if (!transaction.IsInternational || transaction.Route != TransferRoute.FxConversionService)
                {
                    continue;
                }

                if (nowUtc - transaction.InitiatedAtUtc <= InternationalSettlementSla)
                {
                    continue;
                }

                transaction.MarkSettled(transaction.InitiatedAtUtc + InternationalSettlementSla);
            }
        }
    }
}
