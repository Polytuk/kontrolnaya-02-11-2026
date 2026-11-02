using System;
using BodagovKR1_.Interfaces;

namespace BodagovKR1_.Models
{
    /// <summary>
    /// Грузовое транспортное средство корпоративного автопарка.
    /// Демонстрирует наследование, вызов base(), расширение свойств (MaxLoadTon),
    /// переопределение CalculateTax(), явную реализацию интерфейса IExportable
    /// и глубокое клонирование (Deep Copy).
    /// </summary>
    public class CargoVehicle : Vehicle, IExportable
    {
        public double MaxLoadTon { get; private set; }

        public CargoVehicle(int id, string model, int power, decimal basePrice, int categoryId, double maxLoadTon)
            : base(id, model, power, basePrice, categoryId)
        {
            if (maxLoadTon <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLoadTon), "Грузоподъемность должна быть больше нуля.");

            MaxLoadTon = maxLoadTon;
        }

        /// <summary>
        /// Переопределение виртуального метода расчета налога с учетом специфики грузового ТС:
        /// Базовый налог + надбавка за каждую тонну грузоподъемности (повышенный износ дорожного полотна).
        /// </summary>
        public override decimal CalculateTax(decimal taxRate)
        {
            decimal baseTax = base.CalculateTax(taxRate);
            decimal heavyLoadSurcharge = (decimal)MaxLoadTon * 450m;
            return baseTax + heavyLoadSurcharge;
        }

        /// <summary>
        /// ЯВНАЯ РЕАЛИЗАЦИЯ ИНТЕРФЕЙСА (Explicit Interface Implementation).
        /// Метод IExportable.GetExportInfo() доступен ТОЛЬКО при приведении ссылки к IExportable.
        /// Прямой вызов cargoVehicle.GetExportInfo() компилятором запрещен, что обеспечивает инкапсуляцию.
        /// </summary>
        string IExportable.GetExportInfo()
        {
            return $"[EXPLICIT CARGO EXPORT] Грузовик: «{Model}» | Мощность: {Power} л.с. | Грузоподъемность: {MaxLoadTon:F1} т. | Стоимость: {BasePrice:N2} руб.";
        }

        /// <summary>
        /// Глубокое клонирование (Deep Copy):
        /// Создает полностью независимую копию объекта, включая клонирование всех вложенных записей ТО.
        /// Изменение логов в оригинале не затрагивает клон.
        /// </summary>
        public override object Clone()
        {
            var cloned = new CargoVehicle(Id, Model, Power, BasePrice, CategoryId, MaxLoadTon)
            {
                FilePath = this.FilePath
            };

            // Глубокое копирование вложенной коллекции сервисных логов
            foreach (var log in this.MaintenanceRecords)
            {
                cloned.MaintenanceRecords.Add((MaintenanceLog)log.Clone());
            }

            return cloned;
        }

        public override string ToString()
        {
            return $"{base.ToString()} | Грузоподъемность: {MaxLoadTon:F1} т.";
        }
    }
}

