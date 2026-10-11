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

        // נדרש רק כשמנהל מזמין עבור לקוח. לקוח מחובר תמיד מזמין לעצמו
        [Range(1, int.MaxValue)]
        public int? CustomerId { get; set; }

        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }

        // ולידציה שלא תלויה במצב הנתונים. בדיקות זמינות וקיום יושבות ב-Service
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateEnd.Date <= DateStart.Date)
            {
                yield return new ValidationResult(
                    "DateEnd must be at least one day after DateStart",
                    new[] { nameof(DateEnd) });
            }
        }
    }
}
