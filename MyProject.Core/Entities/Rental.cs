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
    }
}
