namespace MyProject.Core.Entities
{
    // תכונה של רכב (אוטומטי, GPS וכו'). קשר many-to-many עם Car
    public class Feature
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
