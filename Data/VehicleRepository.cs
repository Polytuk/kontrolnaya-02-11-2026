using System;
using System.Collections.Generic;
using BodagovKR1_.Interfaces;
using BodagovKR1_.Models;
using Microsoft.Data.Sqlite;

namespace BodagovKR1_.Data
{
    /// <summary>
    /// Репозиторий транспортных средств (Vehicles), реализующий IRepository.
    /// Выполняет полиморфную материализацию объектов CargoVehicle и PassengerVehicle из SQLite.
    /// </summary>
    public class VehicleRepository : IRepository<Vehicle>
    {
        private readonly string _connectionString;

        public VehicleRepository(string? connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseContext.ConnectionString;
        }

        public List<Vehicle> GetAll()
        {
            var vehicles = new List<Vehicle>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Model, BasePrice, Power, CategoryId, VehicleType, ExtraParam FROM Vehicles ORDER BY Id;";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string model = reader.GetString(1);
                decimal basePrice = Convert.ToDecimal(reader.GetDouble(2));
                int power = reader.GetInt32(3);
                int categoryId = reader.GetInt32(4);
                string vehicleType = reader.IsDBNull(5) ? "Passenger" : reader.GetString(5);
                double extra = reader.IsDBNull(6) ? 5.0 : reader.GetDouble(6);

                Vehicle v;
                if (vehicleType.Equals("Cargo", StringComparison.OrdinalIgnoreCase))
                {
                    v = new CargoVehicle(id, model, power, basePrice, categoryId, extra);
                }
                else
                {
                    v = new PassengerVehicle(id, model, power, basePrice, categoryId, (int)extra);
                }

                vehicles.Add(v);
            }

            // Загружаем вложенные сервисные записи (MaintenanceLogs) для каждого ТС
            foreach (var v in vehicles)
            {
                v.MaintenanceRecords = LoadLogsForVehicle(v.Id, connection);
            }

            return vehicles;
        }

        public Vehicle? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Model, BasePrice, Power, CategoryId, VehicleType, ExtraParam FROM Vehicles WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                string model = reader.GetString(1);
                decimal basePrice = Convert.ToDecimal(reader.GetDouble(2));
                int power = reader.GetInt32(3);
                int categoryId = reader.GetInt32(4);
                string vehicleType = reader.IsDBNull(5) ? "Passenger" : reader.GetString(5);
                double extra = reader.IsDBNull(6) ? 5.0 : reader.GetDouble(6);

                Vehicle v = vehicleType.Equals("Cargo", StringComparison.OrdinalIgnoreCase)
                    ? new CargoVehicle(id, model, power, basePrice, categoryId, extra)
                    : new PassengerVehicle(id, model, power, basePrice, categoryId, (int)extra);

                v.MaintenanceRecords = LoadLogsForVehicle(v.Id, connection);
                return v;
            }

            return null;
        }

        public void Add(Vehicle item)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string vehicleType = item is CargoVehicle ? "Cargo" : "Passenger";
            double extra = item is CargoVehicle cv ? cv.MaxLoadTon : (item is PassengerVehicle pv ? pv.PassengerSeats : 5.0);

            string sql = @"
                INSERT INTO Vehicles (Model, BasePrice, Power, CategoryId, VehicleType, ExtraParam)
                VALUES (@Model, @BasePrice, @Power, @CategoryId, @VehicleType, @ExtraParam);
                SELECT last_insert_rowid();
            ";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Model", item.Model);
            cmd.Parameters.AddWithValue("@BasePrice", (double)item.BasePrice);
            cmd.Parameters.AddWithValue("@Power", item.Power);
            cmd.Parameters.AddWithValue("@CategoryId", item.CategoryId);
            cmd.Parameters.AddWithValue("@VehicleType", vehicleType);
            cmd.Parameters.AddWithValue("@ExtraParam", extra);

            long newId = (long)(cmd.ExecuteScalar() ?? 0L);

            // Так как у Id в классе Vehicle закрытый сеттер (get; private set;),
            // если требуется отразить сгенерированный ID в самом переданном объекте:
            var prop = typeof(Vehicle).GetProperty(nameof(Vehicle.Id));
            prop?.SetValue(item, Convert.ToInt32(newId));
        }

        public bool Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM Vehicles WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        private static List<MaintenanceLog> LoadLogsForVehicle(int vehicleId, SqliteConnection connection)
        {
            var logs = new List<MaintenanceLog>();
            string sql = "SELECT Id, VehicleId, ServiceName, Cost, LogDate, Channel FROM MaintenanceLogs WHERE VehicleId = @VehicleId ORDER BY Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@VehicleId", vehicleId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new MaintenanceLog(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    Convert.ToDecimal(reader.GetDouble(3)),
                    reader.GetString(4),
                    reader.GetString(5)
                ));
            }

            return logs;
        }
    }
}

