namespace BodagovKR1_.Models
{
    /// <summary>
    /// Справочник категорий транспортных средств и базовых налоговых ставок.
    /// Соответствует таблице SQLite: Categories.
    /// </summary>
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }

        public Category() { }

        public Category(int id, string categoryName, decimal taxRate)
        {
            Id = id;
            CategoryName = categoryName;
            TaxRate = taxRate;
        }

        public override string ToString() => $"{CategoryName} (Ставка налога: {TaxRate}%)";
    }
}

