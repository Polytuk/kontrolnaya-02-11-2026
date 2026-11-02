# CorpFleet Analytics — Итоговая Контрольная Работа

**Дисциплина:** Объектно-ориентированное программирование на C#  
**Студент:** Бодагов  
**Группа:** ПО-42  
**Дата сдачи:** 02.11.2026  
**Стек технологий:** C# 12 (.NET 8.0-windows), Windows Forms (GUI), SQLite (`corp_fleet.db`), Visual Studio 2022  

---

## Описание проекта
Корпоративная система управления и аналитики транспортного парка компании **«CorpFleet Analytics»**.  
Проект охватывает все ключевые темы курса:
1. **ООП-Архитектура, Инкапсуляция и System.Object:**
   * Базовый абстрактный класс `Vehicle` с закрытыми сеттерами (`get; private set;`) и входной валидацией;
   * Переопределение методов `ToString()`, `Equals(object obj)` и `GetHashCode()`;
   * Классы-наследники `CargoVehicle` (грузоподъемность `MaxLoadTon`) и `PassengerVehicle` (`PassengerSeats`), полиморфный расчет налога `CalculateTax()`.
2. **Обобщения, Контракты Интерфейсов и Клонирование:**
   * Обобщенный интерфейс `IRepository<T> where T : class` (CRUD-операции);
   * Интерфейсы `IStorable` (сохранение в файл) и `IExportable`;
   * **Явная реализация интерфейса** (*Explicit Interface Implementation*) в классе `CargoVehicle` (`IExportable.GetExportInfo()`);
   * Сортировка по базовой стоимости через интерфейс `IComparable`;
   * Истинное **глубокое копирование (Deep Copy)** в `ICloneable` с независимым клонированием вложенной коллекции сервисной истории `MaintenanceRecords`.
3. **Делегаты, События (Events) и Замыкания:**
   * Класс аргументов события `VehicleEventArgs : EventArgs` с read-only свойствами `Message` и `OperationPrice`;
   * Класс-издатель `FleetManager` с событием `StatusChanged` и безопасной генерацией строго через оператор `StatusChanged?.Invoke(...)`;
   * Встроенные функциональные делегаты `Predicate<T>` и `Func<...>`;
   * Демонстрация **замыканий (*Closures*)** с захватом локальных переменных внешней области видимости при обработке списков.
4. **Конвейер LINQ и Взаимодействие с SQLite (`corp_fleet.db`):**
   * Работа с СУБД SQLite через библиотеку `Microsoft.Data.Sqlite` и материализация записей в объекты;
   * Соединение трех сущностей `Vehicles`, `Categories` и `MaintenanceLogs` через `Join` и `GroupJoin` с проекцией в DTO-модель `FleetReportDto`;
   * Фильтрация `Where`, составная сортировка строго через `OrderBy(...).ThenByDescending(...)` и постраничная пагинация `Skip`/`Take`;
   * Группировка и вычисление пяти статистических агрегатов (`GroupBy`, `Count`, `Sum`, `Average`, `Min`, `Max`) и текстовая сводка предприятия через метод `Aggregate`;
   * Операции над множествами (`Except`, `Intersect`, `Union`) по каналам Online и Offline;
   * Предикатные проверки `Any`, `All`, `FirstOrDefault`.
   * **100% чистый LINQ без использования циклов `for/foreach`**.
5. **Графический интерфейс Windows Forms (`FormMain`):**
   * Главное окно на базе компонента `TabControl` с 4 функциональными вкладками:
     1. «1. Реестр ТС и ООП-Модель»
     2. «2. LINQ-Аналитика и DTO»
     3. «3. Группировка, Агрегаты и Множества»
     4. «4. Постраничный просмотр и События» (пагинация и терминальный журнал событий с подпиской через лямбда-выражение).

---

## Структура репозитория
* `BodagovKR1_.sln` — решение Visual Studio;
* `BodagovKR1_.csproj` — файл проекта C# (.NET 8.0-windows);
* `corp_fleet.db` — файловая база данных SQLite с заполненными данными;
* `Program.cs` — точка входа в приложение;
* `VerificationRunner.cs` — автоматический тестовый модуль проверки 21 требования ТЗ;
* `Interfaces/` — контракты интерфейсов (`IRepository`, `IStorable`, `IExportable`);
* `Models/` — доменные модели (`Vehicle`, `CargoVehicle`, `PassengerVehicle`, `Category`, `MaintenanceLog`, `FleetReportDto`);
* `Data/` — контекст базы данных и репозитории;
* `Services/` — бизнес-логика, события и аналитический конвейер LINQ;
* `Forms/` — графический интерфейс Windows Forms;
* `docs/screenshots/` — скриншоты всех 4 вкладок формы;
* `REPORT.md` / `REPORT.html` — полный академический отчет со всеми схемами, листингами и ответами на 5 контрольных вопросов.

---

## Запуск проекта

### В Visual Studio:
1. Откройте `BodagovKR1_.sln`.
2. Нажмите **F5** (или `Ctrl + F5`) для запуска приложения.

### Через командную строку .NET CLI:
```powershell
# Запуск оконного интерфейса:
dotnet run

# Запуск автоматизированной проверки всех требований ТЗ (100% PASS):
dotnet run -- --verify

# Автоматическое сохранение скриншотов всех 4 вкладок:
dotnet run -- --capture
```
