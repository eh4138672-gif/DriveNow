namespace MyProject.Core.Entities
{
    public class Car
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Price { get; set; }

        // מתעדכן בכל הזמנה של הרכב, כדי ששתי הזמנות מקבילות ישנו את אותה שורה
        // ויתנגשו על ה-RowVersion
        public DateTime? LastBookedAt { get; set; }

        // concurrency token - מוגדר כ-rowversion ב-DataContext ומתעדכן אוטומטית בכל שינוי בשורה
        public byte[] RowVersion { get; set; } = default!;

        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
        public ICollection<Feature> Features { get; set; } = new List<Feature>();
    }
}
