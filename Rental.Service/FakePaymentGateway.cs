using Microsoft.Extensions.Logging;
using MyProject.Core.DTOs;
using MyProject.Core.Interfaces;

namespace MyProject.Services
{
    // סימולציה של חברת סליקה: לא מתבצע חיוב אמיתי ולא נשלח שום מידע החוצה.
    // כרטיסי בדיקה (בדומה לכרטיסי הבדיקה של ספקי סליקה אמיתיים):
    //   4242 4242 4242 4242  - מאושר (וכל כרטיס תקין אחר)
    //   4000 0000 0000 0002  - נדחה
    //   4000 0000 0000 9995  - אין מספיק יתרה
    // לא כותבים ללוג את מספר הכרטיס או את ה-CVV, רק את 4 הספרות האחרונות
    public class FakePaymentGateway : IPaymentGateway
    {
        private static readonly TimeSpan ProcessingTime = TimeSpan.FromMilliseconds(300);
        private readonly ILogger<FakePaymentGateway> _logger;

        public FakePaymentGateway(ILogger<FakePaymentGateway> logger)
        {
            _logger = logger;
        }

        public async Task<PaymentResult> ChargeAsync(PaymentDto card, int amount, string description, CancellationToken ct)
        {
            // זמן תגובה של חברת אשראי
            await Task.Delay(ProcessingTime, ct);

            var declineReason = card.Digits switch
            {
                "4000000000000002" => "הכרטיס נדחה על ידי חברת האשראי",
                "4000000000009995" => "אין מספיק יתרה בכרטיס",
                _ => null
            };
            if (declineReason != null)
            {
                _logger.LogWarning("Payment of {Amount} declined for card ending {Last4}", amount, card.Last4);
                return PaymentResult.Declined(declineReason);
            }

            var reference = $"SIM-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
            _logger.LogInformation("Charged {Amount} to card ending {Last4} ({Description}), reference {Reference}",
                amount, card.Last4, description, reference);
            return PaymentResult.Success(reference);
        }

        public Task RefundAsync(string reference, int amount, CancellationToken ct)
        {
            _logger.LogInformation("Refunded {Amount} for payment {Reference}", amount, reference);
            return Task.CompletedTask;
        }
    }
}
