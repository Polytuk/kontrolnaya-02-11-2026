using System;

namespace BodagovKR1_.Services
{
    /// <summary>
    /// Аргументы пользовательского события изменения состояния автопарка.
    /// Содержит свойства Message и OperationPrice только для чтения согласно ТЗ.
    /// </summary>
    public class VehicleEventArgs : EventArgs
    {
        public string Message { get; }
        public decimal OperationPrice { get; }

        public VehicleEventArgs(string message, decimal operationPrice)
        {
            Message = message ?? string.Empty;
            OperationPrice = operationPrice;
        }
    }
}

