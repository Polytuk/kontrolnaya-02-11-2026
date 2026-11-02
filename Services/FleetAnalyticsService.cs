using System;
using System.Collections.Generic;
using System.Linq;
using BodagovKR1_.Models;

namespace BodagovKR1_.Services
{
    /// <summary>
    /// Сервис аналитики автопарка, построенный исключительно на LINQ-конвейерах данных.
    /// Исключает использование циклов for/foreach в соответствии с академическим регламентом ТЗ.
    /// Реализует Join, GroupJoin, Select, Where, OrderBy, ThenByDescending, GroupBy,
    /// Count, Sum, Average, Min, Max, Aggregate, Except, Intersect, Union, Any, All, FirstOrDefault.
    /// </summary>
    public class FleetAnalyticsService
    {
        /// <summary>
        /// Формирование DTO-проекции FleetReportDto путем объединения трех сущностей:
        /// Vehicles (ТС), Categories (Категории) и MaintenanceLogs (Сервисные логи).
        /// Демонстрирует использование Join, GroupJoin и Select.
        /// </summary>
        public List<FleetReportDto> BuildFleetReportDto(
            List<Vehicle> vehicles,
            List<Category> categories,
            List<MaintenanceLog> logs)
        {
            // LINQ Pipeline: Join ТС с категориями -> затем GroupJoin с сервисной историей
            var query = vehicles
                .Join(
                    categories,
                    vehicle => vehicle.CategoryId,
                    category => category.Id,
                    (vehicle, category) => new { Vehicle = vehicle, Category = category }
                )
                .GroupJoin(
                    logs,
                    vc => vc.Vehicle.Id,
                    log => log.VehicleId,
                    (vc, vehicleLogs) => new
                    {
                        vc.Vehicle,
                        vc.Category,
                        VehicleLogs = vehicleLogs
                    }
                )
                .Select(item =>
                {
                    decimal taxRate = item.Category.TaxRate;
                    decimal calculatedTax = item.Vehicle.CalculateTax(taxRate);
                    decimal totalMaintenance = item.VehicleLogs.Sum(l => l.Cost);
                    int maintCount = item.VehicleLogs.Count();
                    decimal totalCost = item.Vehicle.BasePrice + calculatedTax + totalMaintenance;

                    return new FleetReportDto
                    {
                        VehicleId = item.Vehicle.Id,
                        Model = item.Vehicle.Model,
                        VehicleType = item.Vehicle is CargoVehicle ? "Грузовой" : "Легковой",
                        CategoryName = item.Category.CategoryName,
                        Power = item.Vehicle.Power,
                        BasePrice = item.Vehicle.BasePrice,
                        TaxRate = taxRate,
                        CalculatedTax = calculatedTax,
                        TotalMaintenanceCost = totalMaintenance,
                        MaintenanceCount = maintCount,
                        TotalCost = totalCost
                    };
                });

            return query.ToList();
        }

        /// <summary>
        /// Фильтрация и составная сортировка.
        /// СТРОГО: OrderBy(...).ThenByDescending(...) вместо двух OrderBy подряд.
        /// </summary>
        public List<FleetReportDto> FilterAndSort(
            IEnumerable<FleetReportDto> source,
            decimal minPrice,
            decimal maxPrice,
            string searchModel)
        {
            var result = source
                .Where(v => v.BasePrice >= minPrice && v.BasePrice <= maxPrice)
                .Where(v => string.IsNullOrWhiteSpace(searchModel) ||
                            v.Model.Contains(searchModel.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(v => v.CategoryName)
                .ThenByDescending(v => v.BasePrice);

            return result.ToList();
        }

        /// <summary>
        /// Постраничная пагинация данных через Skip и Take.
        /// </summary>
        public List<FleetReportDto> GetPaged(IEnumerable<FleetReportDto> source, int page, int pageSize)
        {
            int safePage = Math.Max(1, page);
            int safePageSize = Math.Max(1, pageSize);

            return source
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToList();
        }

        /// <summary>
        /// Группировка и Агрегация (GroupBy, Count, Sum, Average, Min, Max)
        /// и формирование итоговой текстовой сводки предприятия через метод Aggregate.
        /// </summary>
        public (List<CategoryStatItem> Stats, string AggregateSummary) GetCategoryStatisticsAndSummary(
            IEnumerable<FleetReportDto> reportData)
        {
            var grouped = reportData
                .GroupBy(r => r.CategoryName)
                .Select(g => new CategoryStatItem
                {
                    CategoryName = g.Key,
                    VehicleCount = g.Count(),
                    TotalOperatingCost = g.Sum(x => x.TotalCost),
                    AverageBasePrice = g.Average(x => x.BasePrice),
                    MinBasePrice = g.Min(x => x.BasePrice),
                    MaxBasePrice = g.Max(x => x.BasePrice)
                })
                .ToList();

            // Формирование текстовой сводки предприятия строго через LINQ метод Aggregate
            string header = "=== ИТОГОВАЯ АНАЛИТИЧЕСКАЯ СВОДКА ПРЕДПРИЯТИЯ (CorpFleet Analytics) ===\n";
            string summaryText = grouped
                .Select(s => $"• Категория: [{s.CategoryName}] -> ТС: {s.VehicleCount} ед. | Общие затраты: {s.TotalOperatingCost:N2} руб. | Ср. цена: {s.AverageBasePrice:N2} руб. | Мин: {s.MinBasePrice:N2} руб. | Макс: {s.MaxBasePrice:N2} руб.\n")
                .Aggregate(header, (current, line) => current + line);

            return (grouped, summaryText);
        }

        /// <summary>
        /// Операции над множествами логов: Except, Intersect, Union.
        /// Разделение по каналам 'Online' и 'Offline'.
        /// Корректность обеспечивается переопределением Equals и GetHashCode в MaintenanceLog.
        /// </summary>
        public (List<MaintenanceLog> OnlineOnly,
                List<MaintenanceLog> OfflineOnly,
                List<MaintenanceLog> IntersectLogs,
                List<MaintenanceLog> UnionLogs) PerformSetOperations(List<MaintenanceLog> logs)
        {
            var online = logs.Where(l => l.Channel.Equals("Online", StringComparison.OrdinalIgnoreCase)).ToList();
            var offline = logs.Where(l => l.Channel.Equals("Offline", StringComparison.OrdinalIgnoreCase)).ToList();

            // Except: работы, выполненные ТОЛЬКО через онлайн-канал
            var onlineOnly = online.Except(offline).ToList();

            // Except: работы, выполненные ТОЛЬКО через офлайн-сервис
            var offlineOnly = offline.Except(online).ToList();

            // Intersect: работы, встречающиеся и в онлайн, и в офлайн каналах
            var common = online.Intersect(offline).ToList();

            // Union: полное объединение уникальных сервисных операций
            var union = online.Union(offline).ToList();

            return (onlineOnly, offlineOnly, common, union);
        }

        /// <summary>
        /// Квантификаторы и предикатные проверки: Any, All, FirstOrDefault.
        /// </summary>
        public (bool HasCostOverThreshold, bool AllVehiclesAdequatePower, Vehicle? TopPricedVehicle)
            PerformQuantifierChecks(List<Vehicle> vehicles, List<MaintenanceLog> logs, decimal costThreshold)
        {
            // Any: проверка наличия хотя бы одной сервисной операции дороже порога
            bool hasCostOver = logs.Any(l => l.Cost > costThreshold);

            // All: проверка, что у всех зарегистрированных ТС мощность превышает минимальный порог 50 л.с.
            bool allPower = vehicles.All(v => v.Power >= 50);

            // FirstOrDefault: поиск самого дорогостоящего транспортного средства автопарка
            Vehicle? topVehicle = vehicles.OrderByDescending(v => v.BasePrice).FirstOrDefault();

            return (hasCostOver, allPower, topVehicle);
        }
    }

    /// <summary>
    /// Элемент групповой статистики по категории ТС.
    /// </summary>
    public class CategoryStatItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int VehicleCount { get; set; }
        public decimal TotalOperatingCost { get; set; }
        public decimal AverageBasePrice { get; set; }
        public decimal MinBasePrice { get; set; }
        public decimal MaxBasePrice { get; set; }
    }
}

