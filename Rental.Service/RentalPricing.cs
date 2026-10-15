namespace MyProject.Services
{
    // חיוב לפי שעה: לכל רכב מחיר לשעה בשקלים שלמים, וכל שעה שהתחילה מחויבת כשעה מלאה.
    // השכרה של יותר מיומיים (48 שעות) מקבלת 10% הנחה על המחיר לשעה, על כל השעות.
    // המחיר לשעה אחרי ההנחה מעוגל לשקל שלם, כך שאף פעם לא מוצגים מחירים כמו 13.35
    public static class RentalPricing
    {
        public const int DiscountAfterHours = 48;
        public const decimal LongRentalDiscount = 0.10m;

        public static int BillableHours(DateTime start, DateTime end) =>
            Math.Max(1, (int)Math.Ceiling((end - start).TotalHours));

        public static bool HasLongRentalDiscount(DateTime start, DateTime end) =>
            (end - start).TotalHours > DiscountAfterHours;

        public static int HourlyRate(int pricePerHour, DateTime start, DateTime end) =>
            HasLongRentalDiscount(start, end)
                ? (int)Math.Round(pricePerHour * (1 - LongRentalDiscount), MidpointRounding.AwayFromZero)
                : pricePerHour;

        public static int TotalPrice(int pricePerHour, DateTime start, DateTime end) =>
            BillableHours(start, end) * HourlyRate(pricePerHour, start, end);
    }
}
