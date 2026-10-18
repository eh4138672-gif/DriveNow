namespace MyProject.Core.Entities
{
    public class Rental
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        // DateEnd הוא יום ההחזרה, ולכן השכרה אחרת יכולה להתחיל בו
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        // התשלום על ההזמנה. מהכרטיס נשמרות רק 4 הספרות האחרונות - לא המספר המלא ולא ה-CVV.
        // השכרות ישנות מנתוני ההתחלה (seed) נוצרו לפני שהיה תשלום, ולכן השדות שלהן ריקים
        public int? AmountPaid { get; set; }
        public string? CardLast4 { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
