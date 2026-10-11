using System.ComponentModel.DataAnnotations;

namespace MyProject.Core.DTOs
{
    public class PagingParameters
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

    public class CarQueryParameters : PagingParameters
    {
        // חיפוש לפי דגם או מספר רישוי
        public string? Search { get; set; }

        [Range(0, int.MaxValue)]
        public int? MinPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxPrice { get; set; }

        // רק רכבים עם התכונה הזאת
        public int? FeatureId { get; set; }

        [RegularExpression("(?i)^(model|price|licenseNumber)$", ErrorMessage = "SortBy must be model, price or licenseNumber")]
        public string? SortBy { get; set; }

        public bool Descending { get; set; }
    }

    public class CustomerQueryParameters : PagingParameters
    {
        // חיפוש לפי שם או טלפון
        public string? Search { get; set; }
    }

    public class RentalQueryParameters : PagingParameters
    {
        public int? CarId { get; set; }

        // ללקוח, ה-Service קובע את הערך לפי המשתמש המחובר
        public int? CustomerId { get; set; }

        // השכרות שחופפות לטווח התאריכים
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        // מיון לפי תאריך התחלה
        public bool Descending { get; set; }
    }
}
