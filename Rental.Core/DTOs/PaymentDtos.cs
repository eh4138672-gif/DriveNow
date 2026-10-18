using System.ComponentModel.DataAnnotations;

namespace MyProject.Core.DTOs
{
    // פרטי כרטיס האשראי שנשלחים עם ההזמנה. משמשים לחיוב בלבד ולא נשמרים:
    // בבסיס הנתונים נשמרות רק 4 הספרות האחרונות ומספר האסמכתא
    public class PaymentDto : IValidatableObject
    {
        [Required, StringLength(60, MinimumLength = 2)]
        public string CardHolder { get; set; } = string.Empty;

        // רווחים ומקפים מותרים (כמו שמקלידים מכרטיס). הבדיקה נעשית על הספרות בלבד
        [Required, RegularExpression(@"^[\d -]{12,23}$", ErrorMessage = "CardNumber must contain 12-19 digits")]
        public string CardNumber { get; set; } = string.Empty;

        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Range(2000, 2100)]
        public int ExpiryYear { get; set; }

        [Required, RegularExpression(@"^\d{3,4}$", ErrorMessage = "Cvv must be 3-4 digits")]
        public string Cvv { get; set; } = string.Empty;

        public string Digits => new(CardNumber.Where(char.IsDigit).ToArray());
        public string Last4 => Digits.Length >= 4 ? Digits[^4..] : Digits;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Digits.Length is < 12 or > 19 || !PassesLuhn(Digits))
            {
                yield return new ValidationResult("מספר כרטיס האשראי אינו תקין", new[] { nameof(CardNumber) });
            }

            // הכרטיס תקף עד סוף חודש התוקף
            var now = DateTime.Now;
            if (ExpiryYear < now.Year || (ExpiryYear == now.Year && ExpiryMonth < now.Month))
            {
                yield return new ValidationResult("פג תוקף כרטיס האשראי", new[] { nameof(ExpiryMonth) });
            }
        }

        // אלגוריתם Luhn: ספרת הביקורת של כל כרטיס אשראי. תופס טעויות הקלדה לפני החיוב
        public static bool PassesLuhn(string digits)
        {
            var sum = 0;
            var doubleIt = false;
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var d = digits[i] - '0';
                if (doubleIt)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
                doubleIt = !doubleIt;
            }
            return sum % 10 == 0;
        }
    }

    // תשובת חברת האשראי
    public record PaymentResult(bool Approved, string? Reference, string? DeclineReason)
    {
        public static PaymentResult Success(string reference) => new(true, reference, null);
        public static PaymentResult Declined(string reason) => new(false, null, reason);
    }
}
