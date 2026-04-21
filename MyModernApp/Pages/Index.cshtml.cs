using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyModernApp.Payments;

namespace MyModernApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SettlementEngine _settlementEngine;

        public IndexModel(ILogger<IndexModel> logger, SettlementEngine settlementEngine)
        {
            _logger = logger;
            _settlementEngine = settlementEngine;
        }

        public IReadOnlyList<PaymentTransaction> Transactions { get; private set; } = Array.Empty<PaymentTransaction>();

        public int DelayedBeforeMitigation { get; private set; }

        public int DelayedAfterMitigation { get; private set; }

        public void OnGet()
        {
            var nowUtc = DateTime.UtcNow;
            var transactions = new List<PaymentTransaction>
            {
                new()
                {
                    Id = "INT-0001",
                    IsInternational = true,
                    Route = TransferRoute.FxConversionService,
                    InitiatedAtUtc = nowUtc.AddMinutes(-45)
                },
                new()
                {
                    Id = "INT-0002",
                    IsInternational = true,
                    Route = TransferRoute.FxConversionService,
                    InitiatedAtUtc = nowUtc.AddMinutes(-8)
                },
                new()
                {
                    Id = "DOM-0003",
                    IsInternational = false,
                    Route = TransferRoute.Domestic,
                    InitiatedAtUtc = nowUtc.AddMinutes(-30)
                }
            };

            DelayedBeforeMitigation = CountDelayedPendingSettlements(transactions, nowUtc);

            _settlementEngine.ExpediteDelayedInternationalTransfers(transactions, nowUtc);

            DelayedAfterMitigation = CountDelayedPendingSettlements(transactions, nowUtc);
            Transactions = transactions;
        }

        private static int CountDelayedPendingSettlements(IEnumerable<PaymentTransaction> transactions, DateTime nowUtc)
        {
            return transactions.Count(transaction =>
                transaction.IsInternational
                && transaction.Route == TransferRoute.FxConversionService
                && transaction.Status == SettlementStatus.PendingSettlement
                && nowUtc - transaction.InitiatedAtUtc > SettlementEngine.InternationalSettlementSla);
        }
    }
}
