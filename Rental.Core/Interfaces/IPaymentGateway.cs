using MyProject.Core.DTOs;

namespace MyProject.Core.Interfaces
{
    // חברת הסליקה. בפרויקט יש מימוש מדומה (FakePaymentGateway), ואפשר להחליף אותו
    // בספק אמיתי ברישום ה-DI בלבד, בלי לשנות את RentalService
    public interface IPaymentGateway
    {
        // סכום בשקלים שלמים. מחזיר אסמכתא אם החיוב אושר
        Task<PaymentResult> ChargeAsync(PaymentDto card, int amount, string description, CancellationToken ct);

        // ביטול חיוב (זיכוי), לפי האסמכתא שהתקבלה בחיוב
        Task RefundAsync(string reference, int amount, CancellationToken ct);
    }
}
