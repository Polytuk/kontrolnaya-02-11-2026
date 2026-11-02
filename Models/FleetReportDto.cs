namespace BodagovKR1_.Models
{
    /// <summary>
    /// DTO-проекция комплексного аналитического отчета автопарка.
    /// Формируется в результате LINQ Join и GroupJoin запросов по трем сущностям:
    /// Vehicles, Categories и MaintenanceLogs.
    /// </summary>
    public class FleetReportDto
    {
        public int VehicleId { get; set; }
        public string Model { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Power { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal CalculatedTax { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public int MaintenanceCount { get; set; }
        public decimal TotalCost { get; set; } // BasePrice + CalculatedTax + TotalMaintenanceCost
    }
}

