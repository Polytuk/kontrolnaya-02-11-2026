using System;
using System.Collections.Generic;
using BodagovKR1_.Interfaces;
using BodagovKR1_.Models;
using Microsoft.Data.Sqlite;

namespace BodagovKR1_.Data
{
    /// <summary>
    /// Репозиторий категорий ТС (Categories), реализующий IRepository.
    /// </summary>
    public class CategoryRepository : IRepository<Category>
    {
        private readonly string _connectionString;

        public CategoryRepository(string? connectionString = null)
        {
            _connectionString = connectionString ?? DatabaseContext.ConnectionString;
        }

        public List<Category> GetAll()
        {
            var list = new List<Category>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, CategoryName, TaxRate FROM Categories ORDER BY Id;";
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Category(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    Convert.ToDecimal(reader.GetDouble(2))
                ));
            }

            return list;
        }

        public Category? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, CategoryName, TaxRate FROM Categories WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Category(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    Convert.ToDecimal(reader.GetDouble(2))
                );
            }

            return null;
        }

        public void Add(Category item)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "INSERT INTO Categories (CategoryName, TaxRate) VALUES (@Name, @TaxRate); SELECT last_insert_rowid();";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Name", item.CategoryName);
            cmd.Parameters.AddWithValue("@TaxRate", (double)item.TaxRate);

            item.Id = Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM Categories WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}

