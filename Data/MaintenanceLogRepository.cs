using System;
using System.Collections.Generic;
using BodagovKR1_.Interfaces;
using BodagovKR1_.Models;
using Microsoft.Data.Sqlite;

namespace BodagovKR1_.Data
{
    /// <summary>
    /// Репозиторий сервисных логов (MaintenanceLogs), реализующий IRepository.
    /// </summary>
    public class MaintenanceLogRepository : IRepository<MaintenanceLog>
    {
        private readonly string _connectionString;

        public MaintenanceLogRepository(string? connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseContext.ConnectionString;
        }

        public List<MaintenanceLog> GetAll()
        {
            var logs = new List<MaintenanceLog>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, VehicleId, ServiceName, Cost, LogDate, Channel FROM MaintenanceLogs ORDER BY Id;";
            using var cmd = new SqliteCommand(sql, connection);
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

        public MaintenanceLog? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, VehicleId, ServiceName, Cost, LogDate, Channel FROM MaintenanceLogs WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new MaintenanceLog(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    Convert.ToDecimal(reader.GetDouble(3)),
                    reader.GetString(4),
                    reader.GetString(5)
                );
            }

            return null;
        }

        public void Add(MaintenanceLog item)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"
                INSERT INTO MaintenanceLogs (VehicleId, ServiceName, Cost, LogDate, Channel)
                VALUES (@VehicleId, @ServiceName, @Cost, @LogDate, @Channel);
                SELECT last_insert_rowid();
            ";

            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@VehicleId", item.VehicleId);
            cmd.Parameters.AddWithValue("@ServiceName", item.ServiceName);
            cmd.Parameters.AddWithValue("@Cost", (double)item.Cost);
            cmd.Parameters.AddWithValue("@LogDate", item.LogDate);
            cmd.Parameters.AddWithValue("@Channel", item.Channel);

            item.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM MaintenanceLogs WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}

