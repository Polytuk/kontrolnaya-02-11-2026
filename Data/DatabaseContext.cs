using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace BodagovKR1_.Data
{
    /// <summary>
    /// Контекст работы с файловой СУБД SQLite (corp_fleet.db).
    /// Выполняет инициализацию схемы и первичное наполнение данными.
    /// </summary>
    public static class DatabaseContext
    {
        public static string DatabaseFile => "corp_fleet.db";
        public static string ConnectionString => $"Data Source={DatabaseFile};";

        /// <summary>
        /// Инициализация структуры базы данных и посев тестовых данных при первом запуске.
        /// </summary>
        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // Включаем поддержку внешних ключей в SQLite
            using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                pragmaCmd.ExecuteNonQuery();
            }

            string createTablesSql = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CategoryName TEXT NOT NULL,
                    TaxRate REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Vehicles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Model TEXT NOT NULL,
                    BasePrice REAL NOT NULL,
                    Power INTEGER NOT NULL,
                    CategoryId INTEGER NOT NULL,
                    VehicleType TEXT NOT NULL DEFAULT 'Passenger',
                    ExtraParam REAL NOT NULL DEFAULT 5,
                    FOREIGN KEY(CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS MaintenanceLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VehicleId INTEGER NOT NULL,
                    ServiceName TEXT NOT NULL,
                    Cost REAL NOT NULL,
                    LogDate TEXT NOT NULL,
                    Channel TEXT NOT NULL,
                    FOREIGN KEY(VehicleId) REFERENCES Vehicles(Id) ON DELETE CASCADE
                );
            ";

            using (var cmd = new SqliteCommand(createTablesSql, connection))
            {
                cmd.ExecuteNonQuery();
            }

            // Проверяем, заполнена ли база данных
            using (var checkCmd = new SqliteCommand("SELECT COUNT(*) FROM Categories;", connection))
            {
                long count = (long)(checkCmd.ExecuteScalar() ?? 0L);
                if (count == 0)
                {
                    SeedData(connection);
                }
            }
        }

        /// <summary>
        /// Наполнение базы данных эталонными данными корпоративного парка CorpFleet Analytics.
        /// </summary>
        public static void SeedData(SqliteConnection connection)
        {
            using var transaction = connection.BeginTransaction();

            string seedSql = @"
                -- 1. Справочник категорий
                INSERT INTO Categories (Id, CategoryName, TaxRate) VALUES
                (1, 'Легковой коммерческий транспорт', 5.0),
                (2, 'Магистральный грузовой транспорт', 8.5),
                (3, 'Тяжелая карьерная спецтехника', 12.0),
                (4, 'Экологичный городской электротранспорт', 3.0);

                -- 2. Реестр транспортных средств
                INSERT INTO Vehicles (Id, Model, BasePrice, Power, CategoryId, VehicleType, ExtraParam) VALUES
                (1, 'Skoda Octavia Fleet Pro', 2350000.0, 150, 1, 'Passenger', 5.0),
                (2, 'Lada Largus Cargo Van', 1450000.0, 106, 1, 'Passenger', 7.0),
                (3, 'GAZ Gazelle Next Борт', 3200000.0, 150, 2, 'Cargo', 2.5),
                (4, 'Kamaz 54901 Neo Тягач', 8900000.0, 450, 2, 'Cargo', 20.0),
                (5, 'Volvo FH16 Globetrotter', 14500000.0, 540, 2, 'Cargo', 25.0),
                (6, 'Toyota Camry Executive VIP', 3850000.0, 200, 1, 'Passenger', 5.0),
                (7, 'BelAZ 75710 Mining Hauler', 48000000.0, 2300, 3, 'Cargo', 450.0),
                (8, 'Moskvich 3e Electric EV', 3100000.0, 193, 4, 'Passenger', 5.0),
                (9, 'MAZ 6430 Heavy Hauler', 7400000.0, 420, 2, 'Cargo', 18.0),
                (10, 'MAN TGX EfficientLine', 12100000.0, 480, 2, 'Cargo', 22.0);

                -- 3. Журнал сервисного обслуживания (Каналы Online и Offline)
                -- Включает уникальные онлайн-услуги, уникальные офлайн-работы и пересекающиеся операции
                INSERT INTO MaintenanceLogs (Id, VehicleId, ServiceName, Cost, LogDate, Channel) VALUES
                -- Онлайн операции
                (1, 1, 'Компьютерная телематическая диагностика', 4500.0, '2026-01-15', 'Online'),
                (2, 1, 'Удаленное обновление прошивки ЭБУ', 3000.0, '2026-03-01', 'Online'),
                (3, 2, 'Мониторинг параметров телеметрии', 2500.0, '2026-02-18', 'Online'),
                (4, 3, 'Плановая калибровка датчиков расхода', 5200.0, '2026-02-28', 'Online'),
                (5, 5, 'Дистанционный аудит топливной карты', 3500.0, '2026-01-18', 'Online'),
                (6, 7, 'Удаленная диагностика датчиков перегрузки', 15000.0, '2026-03-10', 'Online'),
                (7, 8, 'Балансировка высоковольтной батареи', 6500.0, '2026-01-30', 'Online'),

                -- Офлайн операции
                (8, 1, 'Замена моторного масла и фильтров', 9500.0, '2026-02-10', 'Offline'),
                (9, 2, 'Замена тормозных колодок и дисков', 14200.0, '2026-01-20', 'Offline'),
                (10, 3, 'Плановое ТО-1 ходовой части', 22000.0, '2026-01-25', 'Offline'),
                (11, 4, 'Диагностика пневмоподвески и тормозов', 35000.0, '2026-02-05', 'Offline'),
                (12, 4, 'Замена сцепления и регулировка КПП', 78000.0, '2026-04-02', 'Offline'),
                (13, 5, 'Замена моторного масла и фильтров', 28500.0, '2026-03-15', 'Offline'),
                (14, 6, 'Плановое сервисное ТО-2', 18500.0, '2026-03-20', 'Offline'),
                (15, 7, 'Капитальный ремонт гидравлической системы', 350000.0, '2026-02-15', 'Offline'),
                (16, 9, 'Замена моторного масла и фильтров', 24000.0, '2026-02-22', 'Offline'),
                (17, 10, 'Регулировка углов установки колес и сход-развал', 19000.0, '2026-03-05', 'Offline'),

                -- Пересекающиеся операции (выполняются как в Online, так и в Offline каналах с одинаковой стоимостью)
                (18, 4, 'Компьютерная телематическая диагностика', 4500.0, '2026-03-12', 'Offline'),
                (19, 9, 'Плановая калибровка датчиков расхода', 5200.0, '2026-03-18', 'Offline'),
                (20, 8, 'Балансировка высоковольтной батареи', 6500.0, '2026-03-25', 'Offline');
            ";

            using (var cmd = new SqliteCommand(seedSql, connection, transaction))
            {
                cmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
