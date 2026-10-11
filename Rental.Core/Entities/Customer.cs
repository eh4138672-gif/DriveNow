namespace MyProject.Core.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DriverLicenseId { get; set; } = string.Empty;

        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    }
}
