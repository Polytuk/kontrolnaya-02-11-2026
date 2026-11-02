using System;

namespace BodagovKR1_.Models
{
    /// <summary>
    /// Журнал сервисного обслуживания ТС.
    /// Соответствует таблице SQLite: MaintenanceLogs.
    /// Переопределяет Equals и GetHashCode для корректной работы операций над множествами (Except, Intersect, Union).
    /// </summary>
    public class MaintenanceLog : ICloneable, IEquatable<MaintenanceLog>
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string LogDate { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty; // 'Online' или 'Offline'

        public MaintenanceLog() { }

        public MaintenanceLog(int id, int vehicleId, string serviceName, decimal cost, string logDate, string channel)
        {
            Id = id;
            VehicleId = vehicleId;
            ServiceName = serviceName;
            Cost = cost;
            LogDate = logDate;
            Channel = channel;
        }

        public object Clone()
        {
            return new MaintenanceLog(Id, VehicleId, ServiceName, Cost, LogDate, Channel);
        }

        public override string ToString()
        {
            return $"[{Channel}] {ServiceName} - {Cost:N2} руб. ({LogDate})";
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as MaintenanceLog);
        }

        public bool Equals(MaintenanceLog? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            // Услуги считаются идентичными, если совпадают наименование услуги и стоимость
            return string.Equals(ServiceName, other.ServiceName, StringComparison.OrdinalIgnoreCase) && Cost == other.Cost;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ServiceName?.ToLowerInvariant(), Cost);
        }
    }
}

