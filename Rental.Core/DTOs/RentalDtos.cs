using System.ComponentModel.DataAnnotations;

namespace MyProject.Core.DTOs
{
    public class RentalDto
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public string CarModel { get; set; } = string.Empty;
        public string CarLicenseNumber { get; set; } = string.Empty;
        public List<string> CarFeatures { get; set; } = new();
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int TotalPrice { get; set; }
    }

    public class RentalRequestDto : IValidatableObject
    {
        [Range(1, int.MaxValue)]
        public int CarId { get; set; }

        // ההזמנה נרשמת על שם המשתמש המחובר. כדי להזמין על שם לקוח אחר, שולחים את מספר
        // רישיון הנהיגה שלו (התאמה מדויקת) - כך אפשר להזמין עבור מישהו בלי לראות רשימת לקוחות
        [RegularExpression(@"^\d{7,9}$", ErrorMessage = "DriverLicenseId must be 7-9 digits")]
        public string? DriverLicenseId { get; set; }

        // לשימוש מנהל דרך ה-API בלבד. לקוח שישלח מזהה אחר - המזהה יתעלם
        [Range(1, int.MaxValue)]
        public int? CustomerId { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        // ולידציה שלא תלויה במצב הנתונים. בדיקות זמינות וקיום יושבות ב-Service
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // ההשכרה כוללת תאריך ושעה. מינימום שעה אחת
            if (DateEnd < DateStart.AddHours(1))
            {
                yield return new ValidationResult(
                    "DateEnd must be at least one hour after DateStart",
                    new[] { nameof(DateEnd) });
            }
        }
    }
}
