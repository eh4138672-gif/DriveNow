using System.ComponentModel.DataAnnotations;

namespace MyProject.Core.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DriverLicenseId { get; set; } = string.Empty;
    }

    public class CustomerRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^0\d{1,2}-?\d{7}$", ErrorMessage = "PhoneNumber must look like 050-1234567")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{7,9}$", ErrorMessage = "DriverLicenseId must be 7-9 digits")]
        public string DriverLicenseId { get; set; } = string.Empty;
    }
}
