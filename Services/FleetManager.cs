using System;
using System.Collections.Generic;
using System.Linq;
using BodagovKR1_.Data;
using BodagovKR1_.Models;

namespace BodagovKR1_.Services
{
    /// <summary>
    /// Класс-издатель корпоративной системы автопарка (FleetManager).
    /// Управляет событиями, делегатами, замыканиями и взаимодействием с репозиториями.
    /// </summary>
    public class FleetManager
    {
        private readonly VehicleRepository _vehicleRepo;
        private readonly MaintenanceLogRepository _logRepo;
        private readonly CategoryRepository _categoryRepo;

        /// <summary>
        /// Событие изменения состояния ТС.
        /// Сигнатура Action<object, VehicleEventArgs> с оператором безопасного вызова ?.
        /// </summary>
        public event Action<object, VehicleEventArgs>? StatusChanged;

        public FleetManager(
            VehicleRepository vehicleRepo,
            MaintenanceLogRepository logRepo,
            CategoryRepository categoryRepo)
        {
            _vehicleRepo = vehicleRepo;
            _logRepo = logRepo;
            _categoryRepo = categoryRepo;
        }

        /// <summary>
        /// Добавление нового ТС в автопарк с безопасной генерацией события.
        /// </summary>
        public void AddVehicle(Vehicle vehicle)
        {
            _vehicleRepo.Add(vehicle);

            // Генерация события строго через оператор безопасного вызова ?.Invoke(...)
            StatusChanged?.Invoke(this, new VehicleEventArgs(
                $"Добавлено новое ТС: {vehicle.Model} (ID #{vehicle.Id})",
                vehicle.BasePrice));
        }

        /// <summary>
        /// Удаление ТС из автопарка с уведомлением подписчиков.
        /// </summary>
        public bool RemoveVehicle(int vehicleId)
        {
            var vehicle = _vehicleRepo.GetById(vehicleId);
            if (vehicle == null) return false;

            bool deleted = _vehicleRepo.Delete(vehicleId);
            if (deleted)
            {
                StatusChanged?.Invoke(this, new VehicleEventArgs(
                    $"ТС #{vehicleId} «{vehicle.Model}» списано и удалено из реестра",
                    vehicle.BasePrice));
            }
            return deleted;
        }

        /// <summary>
        /// Регистрация сервисной операции ТО.
        /// </summary>
        public void RegisterMaintenance(int vehicleId, string serviceName, decimal cost, string channel)
        {
            var log = new MaintenanceLog(0, vehicleId, serviceName, cost, DateTime.Now.ToString("yyyy-MM-dd"), channel);
            _logRepo.Add(log);

            // Потокобезопасная генерация события
            StatusChanged?.Invoke(this, new VehicleEventArgs(
                $"Выполнено ТО для ТС #{vehicleId}: {serviceName} [{channel}]",
                cost));
        }

        /// <summary>
        /// Демонстрация использования встроенного делегата Predicate<T>
        /// </summary>
        public List<Vehicle> FilterVehicles(List<Vehicle> list, Predicate<Vehicle> predicate)
        {
            // Использование LINQ Where с делегатом Predicate
            return list.Where(v => predicate(v)).ToList();
        }

        /// <summary>
        /// Демонстрация использования встроенного делегата Func<...>
        /// </summary>
        public decimal CalculateCustomTotal(List<Vehicle> list, Func<Vehicle, decimal> priceCalculator)
        {
            // LINQ Sum с делегатом Func
            return list.Sum(v => priceCalculator(v));
        }

        /// <summary>
        /// ДЕМОНСТРАЦИЯ ЗАМЫКАНИЯ (CLOSURE):
        /// Захват локальных переменных внешней области видимости (priceFloor и taxMultiplier)
        /// внутри анонимных методов/лямбда-выражений при обработке списков ТС.
        /// </summary>
        public (List<Vehicle> matchedVehicles, decimal totalTaxWithSubsidy, string closureInfo) RunClosureAnalysis(
            List<Vehicle> vehicles,
            decimal priceFloor,
            decimal taxSubsidyPercent)
        {
            // Локальные переменные внешней области видимости стека метода
            decimal capturedFloor = priceFloor;
            decimal capturedSubsidyRate = (100m - taxSubsidyPercent) / 100m;

            // Лямбда 1: Замыкание захватывает capturedFloor
            Predicate<Vehicle> priceFilter = v => v.BasePrice >= capturedFloor;

            // Лямбда 2: Замыкание захватывает capturedSubsidyRate
            Func<Vehicle, decimal> subsidyTaxCalc = v =>
            {
                // Расчет налога с учетом внешней льготы субсидирования
                decimal nominalTax = v.CalculateTax(8.0m);
                return nominalTax * capturedSubsidyRate;
            };

            // Применение фильтрации и расчета через захваченные переменные
            var matched = vehicles.Where(v => priceFilter(v)).ToList();
            decimal totalSubsidizedTax = matched.Sum(subsidyTaxCalc);

            string info = $"[Замыкание / Closure]: Захвачен порог цены >= {capturedFloor:N0} руб. " +
                          $"и коэффициент субсидии = {capturedSubsidyRate:P1}. Отобрано {matched.Count} ТС.";

            // Оповещение подписчиков
            StatusChanged?.Invoke(this, new VehicleEventArgs(
                $"Выполнен расчет субсидий через замыкание (захват порога {capturedFloor:N0} руб.)",
                totalSubsidizedTax));

            return (matched, totalSubsidizedTax, info);
        }
    }
}

