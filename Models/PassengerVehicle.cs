using System;

namespace BodagovKR1_.Models
{
    /// <summary>
    /// Легковое транспортное средство корпоративного автопарка.
    /// Демонстрирует наследование, полиморфизм (CalculateTax),
    /// неявную реализацию интерфейса и глубокое копирование (Deep Copy).
    /// </summary>
    public class PassengerVehicle : Vehicle
    {
        public int PassengerSeats { get; private set; }

        public PassengerVehicle(int id, string model, int power, decimal basePrice, int categoryId, int passengerSeats = 5)
            : base(id, model, power, basePrice, categoryId)
        {
            if (passengerSeats <= 0)
                throw new ArgumentOutOfRangeException(nameof(passengerSeats), "Количество посадочных мест должно быть больше нуля.");

            PassengerSeats = passengerSeats;
        }

        /// <summary>
        /// Переопределение виртуального метода расчета налога:
        /// Базовый налог + дифференцированная шкала транспортного налога по мощности двигателя.
        /// </summary>
        public override decimal CalculateTax(decimal taxRate)
        {
            decimal baseTax = base.CalculateTax(taxRate);
            // Дополнительная ставка в зависимости от мощности л.с.
            decimal powerTax = Power switch
            {
                > 250 => Power * 150m,
                > 150 => Power * 50m,
                > 100 => Power * 35m,
                _ => Power * 25m
            };

            return baseTax + powerTax;
        }

        /// <summary>
        /// Обычная реализация метода интерфейса IExportable.
        /// </summary>
        public override string GetExportInfo()
        {
            return $"[PASSENGER EXPORT] Легковой авто: «{Model}» | Мощность: {Power} л.с. | Мест: {PassengerSeats} | Базовая цена: {BasePrice:N2} руб.";
        }

        /// <summary>
        /// Глубокое клонирование (Deep Copy) легкового автомобиля и его истории обслуживания.
        /// </summary>
        public override object Clone()
        {
            var cloned = new PassengerVehicle(Id, Model, Power, BasePrice, CategoryId, PassengerSeats)
            {
                FilePath = this.FilePath
            };

            // Глубокое копирование вложенных сервисных объектов
            foreach (var log in this.MaintenanceRecords)
            {
                cloned.MaintenanceRecords.Add((MaintenanceLog)log.Clone());
            }

            return cloned;
        }

        public override string ToString()
        {
            return $"{base.ToString()} | Мест: {PassengerSeats}";
        }
    }
}

