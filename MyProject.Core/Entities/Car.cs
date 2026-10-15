namespace MyProject.Core.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        // מחיר לשעה בשקלים שלמים (מינימום 30). המחיר הכולל מחושב ב-RentalPricing
        public int PricePerHour { get; set; }

        // רכב שהוצא מההשכרה לא מוצג ללקוחות ולא ניתן להזמנה, אבל ההיסטוריה שלו נשמרת
        public bool IsActive { get; set; } = true;

        // מתעדכן בכל הזמנה של הרכב, כדי ששתי הזמנות מקבילות ישנו את אותה שורה
        // ויתנגשו על ה-Version
        public DateTime? LastBookedAt { get; set; }

        // concurrency token. ב-PostgreSQL אין rowversion, ולכן זה ערך שהמערכת מנהלת בעצמה:
        // DataContext מחליף אותו בכל שמירה של הרכב, ו-EF מוסיף את הערך הישן לתנאי ה-WHERE של ה-UPDATE
        public Guid Version { get; set; } = Guid.NewGuid();

        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
    }
}
