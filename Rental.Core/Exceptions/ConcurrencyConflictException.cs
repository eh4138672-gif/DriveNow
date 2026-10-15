namespace MyProject.Core.Exceptions
{
    // נזרקת מה-Repository כשהשמירה נכשלה כי מישהו אחר שינה את השורה בין הקריאה לשמירה.
    // עוטפת את DbUpdateConcurrencyException, כדי ששכבת ה-Service לא תהיה תלויה ב-EF Core
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
