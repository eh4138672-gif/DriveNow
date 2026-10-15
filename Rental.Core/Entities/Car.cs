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
        // ויתנגשו על ה-RowVersion
        public DateTime? LastBookedAt { get; set; }

        // concurrency token - מוגדר כ-rowversion ב-DataContext ומתעדכן אוטומטית בכל שינוי בשורה
        public byte[] RowVersion { get; set; } = default!;

        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
    }
}
