using System;
using System.Collections.Generic;
using System.IO;
using BodagovKR1_.Interfaces;

namespace BodagovKR1_.Models
{
    /// <summary>
    /// Абстрактный базовый класс транспортного средства корпоративного парка.
    /// Демонстрирует инкапсуляцию (закрытые сеттеры), полиморфизм, переопределение System.Object,
    /// контракты интерфейсов IComparable, ICloneable, IStorable, IExportable.
    /// </summary>
    public abstract class Vehicle : IComparable<Vehicle>, ICloneable, IStorable, IExportable
    {
        // Свойства с закрытыми сеттерами согласно требованию ТЗ (get; private set;)
        public int Id { get; private set; }
        public string Model { get; private set; }
        public int Power { get; private set; }
        public decimal BasePrice { get; private set; }
        public int CategoryId { get; private set; }

        // Вложенный объект для демонстрации глубокого копирования (Deep Copy)
        public List<MaintenanceLog> MaintenanceRecords { get; set; } = new();

        // Свойство интерфейса IStorable
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Конструктор с проверкой валидности входных параметров.
        /// </summary>
        protected Vehicle(int id, string model, int power, decimal basePrice, int categoryId)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Идентификатор ID не может быть отрицательным.");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Наименование модели не может быть пустым.", nameof(model));

            if (power <= 0)
                throw new ArgumentOutOfRangeException(nameof(power), "Мощность двигателя должна быть строго больше нуля.");

            if (basePrice < 0)
                throw new ArgumentOutOfRangeException(nameof(basePrice), "Базовая стоимость не может быть отрицательной.");

            if (categoryId <= 0)
                throw new ArgumentOutOfRangeException(nameof(categoryId), "Идентификатор категории должен быть положительным числом.");

            Id = id;
            Model = model.Trim();
            Power = power;
            BasePrice = basePrice;
            CategoryId = categoryId;
            FilePath = $"exports/vehicle_{id}.txt";
        }

        /// <summary>
        /// Виртуальный метод расчета налога с учетом ставки TaxRate из категории.
        /// </summary>
        public virtual decimal CalculateTax(decimal taxRate)
        {
            return BasePrice * (taxRate / 100m);
        }

        /// <summary>
        /// Переопределение System.Object: ToString().
        /// </summary>
        public override string ToString()
        {
            return $"[{GetType().Name}] #{Id} «{Model}» | Мощность: {Power} л.с. | Цена: {BasePrice:N2} руб.";
        }

        /// <summary>
        /// Переопределение System.Object: Equals().
        /// ТЗ: Две машины считаются равными, если совпадают их Model и Power.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not Vehicle other)
                return false;

            return string.Equals(Model, other.Model, StringComparison.OrdinalIgnoreCase) && Power == other.Power;
        }

        /// <summary>
        /// Переопределение System.Object: GetHashCode().
        /// Согласовано с Equals() по полям Model и Power.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(Model?.ToLowerInvariant(), Power);
        }

        /// <summary>
        /// Реализация IComparable: автоматическая сортировка по базовой стоимости BasePrice.
        /// </summary>
        public int CompareTo(Vehicle? other)
        {
            if (other is null) return 1;
            return BasePrice.CompareTo(other.BasePrice);
        }

        /// <summary>
        /// Контракт глубокого клонирования (ICloneable).
        /// </summary>
        public abstract object Clone();

        /// <summary>
        /// Реализация метода сохранения IStorable.
        /// </summary>
        public virtual void Save()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                FilePath = $"exports/vehicle_{Id}.txt";
            }

            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(FilePath, ToString());
        }

        /// <summary>
        /// Метод интерфейса IExportable.
        /// </summary>
        public virtual string GetExportInfo()
        {
            return $"Экспорт ТС #{Id}: {Model}, Мощность: {Power} л.с., Цена: {BasePrice:N2} руб.";
        }
    }
}

