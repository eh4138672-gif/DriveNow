using System.ComponentModel.DataAnnotations;

namespace MyProject.Core.DTOs
{
    public class CarDto
    {
        public int Id { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int PricePerHour { get; set; }
        public bool IsActive { get; set; }
        public List<FeatureDto> Features { get; set; } = new();

        // נשלח בחזרה בעדכון, כדי לזהות שינוי שקרה מאז שהלקוח קרא את הרכב
        public byte[] RowVersion { get; set; } = default!;
    }

    public class CarRequestDto
    {
        [Required]
        [RegularExpression(@"^\d{2,3}-\d{2,3}-\d{2,3}$", ErrorMessage = "LicenseNumber must look like 123-45-678")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Model { get; set; } = string.Empty;

        // מחיר לשעה, בשקלים שלמים
        [Range(30, 1000, ErrorMessage = "PricePerHour must be between 30 and 1000")]
        public int PricePerHour { get; set; }

        public List<int> FeatureIds { get; set; } = new();
    }

    public class CarUpdateDto : CarRequestDto
    {
        [Required]
        public byte[] RowVersion { get; set; } = default!;
    }

    // הוצאת רכב מההשכרה או החזרתו
    public class CarAvailabilityDto
    {
        [Required]
        public bool? IsActive { get; set; }
    }
}
