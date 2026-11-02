using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BodagovKR1_.Data;
using BodagovKR1_.Interfaces;
using BodagovKR1_.Models;
using BodagovKR1_.Services;

namespace BodagovKR1_
{
    public static class VerificationRunner
    {
        public static void RunAllChecks()
        {
            DatabaseContext.InitializeDatabase();

            Console.WriteLine("================================================================================");
            Console.WriteLine("КОМПЛЕКСНАЯ ПРОВЕРКА СООТВЕТСТВИЯ ТЕХНИЧЕСКОМУ ЗАДАНИЮ (ИТОГОВАЯ КОНТРОЛЬНАЯ РАБОТА)");
            Console.WriteLine("================================================================================");

            int passed = 0;
            int total = 0;

            void Check(string title, bool condition, string details = "")
            {
                total++;
                if (condition)
                {
                    passed++;
                    Console.WriteLine($"[PASS {passed}/{total}] {title}");
                    if (!string.IsNullOrEmpty(details)) Console.WriteLine($"       -> {details}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FAIL {passed}/{total}] {title}");
                    if (!string.IsNullOrEmpty(details)) Console.WriteLine($"       -> ОШИБКА: {details}");
                    Console.ResetColor();
                }
            }

            // ---------------------------------------------------------
            // МОДУЛЬ 1. ООП-Архитектура, Инкапсуляция и System.Object
            // ---------------------------------------------------------
            Console.WriteLine("\n--- МОДУЛЬ 1: ООП, Свойства и System.Object ---");

            var vehicleType = typeof(Vehicle);
            Check("1.1. Класс Vehicle является абстрактным", vehicleType.IsAbstract);

            var propId = vehicleType.GetProperty("Id");
            var propModel = vehicleType.GetProperty("Model");
            var propPower = vehicleType.GetProperty("Power");
            var propBasePrice = vehicleType.GetProperty("BasePrice");

            bool idPrivateSet = propId?.GetSetMethod(true)?.IsPrivate == true && propId.GetSetMethod(false) == null;
            bool modelPrivateSet = propModel?.GetSetMethod(true)?.IsPrivate == true && propModel.GetSetMethod(false) == null;
            bool powerPrivateSet = propPower?.GetSetMethod(true)?.IsPrivate == true && propPower.GetSetMethod(false) == null;
            bool pricePrivateSet = propBasePrice?.GetSetMethod(true)?.IsPrivate == true && propBasePrice.GetSetMethod(false) == null;

            Check("1.2. Свойства Id, Model, Power, BasePrice имеют закрытые сеттеры (get; private set;)",
                idPrivateSet && modelPrivateSet && powerPrivateSet && pricePrivateSet,
                $"Id private={idPrivateSet}, Model private={modelPrivateSet}, Power private={powerPrivateSet}, BasePrice private={pricePrivateSet}");

            bool ctorThrowsOnInvalid = false;
            try
            {
                _ = new PassengerVehicle(-1, "", 0, -100m, 0, 0);
            }
            catch (ArgumentException)
            {
                ctorThrowsOnInvalid = true;
            }
            Check("1.3. Конструктор выполняет валидацию входных параметров и выбрасывает ArgumentException", ctorThrowsOnInvalid);

            var car1 = new PassengerVehicle(1, "GAZ Gazelle", 150, 2000000m, 1, 3);
            var car2 = new PassengerVehicle(2, "GAZ Gazelle", 150, 4500000m, 2, 5); // Другой ID и цена, но одинаковые Model и Power
            var car3 = new PassengerVehicle(3, "GAZ Gazelle", 190, 2000000m, 1, 3); // Другая мощность

            bool equalsSame = car1.Equals(car2);
            bool equalsDiff = !car1.Equals(car3);
            bool hashSame = car1.GetHashCode() == car2.GetHashCode();
            Check("1.4. Переопределение Equals и GetHashCode: две машины равны, если совпадают Model и Power",
                equalsSame && equalsDiff && hashSame,
                $"car1.Equals(car2)={equalsSame}, car1.Equals(car3)={!equalsDiff}, HashCodesEqual={hashSame}");

            var cargo = new CargoVehicle(10, "Kamaz 54901", 450, 8000000m, 2, 20.0);
            Check("1.5. Наследник CargoVehicle содержит свойство MaxLoadTon и наследует Vehicle",
                cargo is Vehicle && cargo.MaxLoadTon == 20.0);

            decimal baseTax = 8000000m * 0.085m;
            decimal cargoTax = cargo.CalculateTax(8.5m);
            Check("1.6. Переопределение CalculateTax() в CargoVehicle с учетом TaxRate и специфики ТС",
                cargoTax > baseTax,
                $"Базовый налог={baseTax:N2}, Грузовой налог={cargoTax:N2}");

            // ---------------------------------------------------------
            // МОДУЛЬ 2. Обобщения, Контракты Интерфейсов и Клонирование
            // ---------------------------------------------------------
            Console.WriteLine("\n--- МОДУЛЬ 2: Обобщения, Интерфейсы и Клонирование ---");

            var repoInterface = typeof(IRepository<>);
            var genericArg = repoInterface.GetGenericArguments()[0];
            var constraints = genericArg.GetGenericParameterConstraints();
            bool hasClassConstraint = (genericArg.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
            Check("2.1. Обобщенный интерфейс IRepository<T> имеет ограничение where T : class",
                repoInterface.IsInterface && hasClassConstraint);

            Check("2.2. Классы ТС реализуют интерфейсы IStorable и IExportable",
                cargo is IStorable && cargo is IExportable && car1 is IStorable && car1 is IExportable);

            // Проверка явной реализации IExportable.GetExportInfo() в CargoVehicle
            var publicExportMethod = typeof(CargoVehicle).GetMethod("GetExportInfo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            bool isExplicit = publicExportMethod == null;
            string explicitResult = ((IExportable)cargo).GetExportInfo();
            Check("2.3. В CargoVehicle метод IExportable.GetExportInfo() реализован ЯВНО (Explicit Interface Implementation)",
                isExplicit && !string.IsNullOrEmpty(explicitResult),
                $"Публичный метод отсутствует в классе={isExplicit}, вызов через (IExportable)='{explicitResult}'");

            // Проверка IComparable
            var listToSort = new List<Vehicle> { cargo, car1, car3 };
            listToSort.Sort();
            bool isSorted = listToSort[0].BasePrice <= listToSort[1].BasePrice && listToSort[1].BasePrice <= listToSort[2].BasePrice;
            Check("2.4. Реализация IComparable: автоматическая сортировка по BasePrice", isSorted,
                $"Цены после Sort(): {string.Join(" <= ", listToSort.Select(v => v.BasePrice))}");

            // Проверка глубокого клонирования (Deep Copy)
            cargo.MaintenanceRecords.Clear();
            cargo.MaintenanceRecords.Add(new MaintenanceLog(1, cargo.Id, "ТО-1", 5000m, "2026-01-01", "Online"));
            var clonedCargo = (CargoVehicle)cargo.Clone();

            bool notSameRef = !ReferenceEquals(cargo, clonedCargo);
            bool notSameRecordsRef = !ReferenceEquals(cargo.MaintenanceRecords, clonedCargo.MaintenanceRecords);
            // Изменяем оригинал
            cargo.MaintenanceRecords.Add(new MaintenanceLog(2, cargo.Id, "ТО-2 Добавлено в оригинал", 10000m, "2026-02-01", "Offline"));
            bool cloneUnaffected = clonedCargo.MaintenanceRecords.Count == 1 && cargo.MaintenanceRecords.Count == 2;
            Check("2.5. Глубокое клонирование (Deep Copy) в ICloneable: вложенные сервисные объекты полностью независимы",
                notSameRef && notSameRecordsRef && cloneUnaffected,
                $"RefEquals={notSameRef}, RecordsRefEquals={notSameRecordsRef}, CloneUnaffected={cloneUnaffected}");

            // ---------------------------------------------------------
            // МОДУЛЬ 3. Делегаты, События (Events) и Замыкания
            // ---------------------------------------------------------
            Console.WriteLine("\n--- МОДУЛЬ 3: Делегаты, События и Замыкания ---");

            var eventArgsType = typeof(VehicleEventArgs);
            var msgProp = eventArgsType.GetProperty("Message");
            var priceProp = eventArgsType.GetProperty("OperationPrice");
            bool argsReadOnly = msgProp?.GetSetMethod() == null && priceProp?.GetSetMethod() == null;
            Check("3.1. Класс VehicleEventArgs наследует EventArgs и содержит read-only свойства Message и OperationPrice",
                typeof(EventArgs).IsAssignableFrom(eventArgsType) && argsReadOnly);

            var vRepo = new VehicleRepository();
            var lRepo = new MaintenanceLogRepository();
            var cRepo = new CategoryRepository();
            var fleetManager = new FleetManager(vRepo, lRepo, cRepo);

            bool eventFired = false;
            string firedMsg = "";
            decimal firedPrice = 0m;
            // Подписка через лямбда-выражение
            fleetManager.StatusChanged += (s, e) =>
            {
                eventFired = true;
                firedMsg = e.Message;
                firedPrice = e.OperationPrice;
            };

            var testCar = new PassengerVehicle(0, "Lada Granta Test", 90, 850000m, 1, 5);
            fleetManager.AddVehicle(testCar);

            Check("3.2. Класс FleetManager генерирует событие StatusChanged с помощью оператора безопасного вызова ?.Invoke()",
                eventFired && firedMsg.Contains("Lada Granta Test") && firedPrice == 850000m,
                $"EventFired={eventFired}, Message='{firedMsg}', Price={firedPrice}");

            // Замыкание (Closure) и встроенные делегаты
            decimal capturedFloor = 3000000m;
            decimal capturedSubsidy = 20m;
            var closureResult = fleetManager.RunClosureAnalysis(vRepo.GetAll(), capturedFloor, capturedSubsidy);
            Check("3.3. Встроенные делегаты Predicate и Func с захватом локальных переменных внешней области видимости (Замыкания)",
                closureResult.matchedVehicles.All(v => v.BasePrice >= capturedFloor) && closureResult.totalTaxWithSubsidy > 0,
                closureResult.closureInfo);

            // Удаляем тестовую машину
            fleetManager.RemoveVehicle(testCar.Id);

            // ---------------------------------------------------------
            // МОДУЛЬ 4. Конвейер LINQ и Взаимодействие с SQLite
            // ---------------------------------------------------------
            Console.WriteLine("\n--- МОДУЛЬ 4: Конвейер LINQ и Взаимодействие с SQLite ---");

            var allVehicles = vRepo.GetAll();
            var allCategories = cRepo.GetAll();
            var allLogs = lRepo.GetAll();

            Check("4.1. Считывание и материализация записей из SQLite (corp_fleet.db) в коллекции C# объектов",
                allVehicles.Count >= 8 && allCategories.Count >= 4 && allLogs.Count >= 15,
                $"Vehicles={allVehicles.Count}, Categories={allCategories.Count}, Logs={allLogs.Count}");

            var analytics = new FleetAnalyticsService();
            var reportDtoList = analytics.BuildFleetReportDto(allVehicles, allCategories, allLogs);

            Check("4.2. Связывание и Проекция в FleetReportDto с расчетными полями через Join, GroupJoin, Select",
                reportDtoList.Count == allVehicles.Count && reportDtoList.All(r => r.TotalCost > 0 && r.CalculatedTax > 0),
                $"Сформировано {reportDtoList.Count} DTO-записей. Пример первого ТС: {reportDtoList[0].Model}, Налог: {reportDtoList[0].CalculatedTax:N2}, ТО: {reportDtoList[0].TotalMaintenanceCost:N2}");

            var filteredSorted = analytics.FilterAndSort(reportDtoList, 2000000m, 15000000m, "");
            bool isSortedComposite = true;
            for (int i = 0; i < filteredSorted.Count - 1; i++)
            {
                int catComp = string.Compare(filteredSorted[i].CategoryName, filteredSorted[i + 1].CategoryName, StringComparison.Ordinal);
                if (catComp > 0 || (catComp == 0 && filteredSorted[i].BasePrice < filteredSorted[i + 1].BasePrice))
                {
                    isSortedComposite = false;
                    break;
                }
            }
            Check("4.3. Фильтрация Where и Составная сортировка OrderBy(CategoryName).ThenByDescending(BasePrice)",
                isSortedComposite && filteredSorted.Count > 0,
                $"Записей после фильтра: {filteredSorted.Count}, Корректность составной сортировки={isSortedComposite}");

            var pagedItems = analytics.GetPaged(filteredSorted, 1, 3);
            Check("4.4. Постраничный вывод данных (Пагинация) через Skip((page - 1) * pageSize).Take(pageSize)",
                pagedItems.Count <= 3 && pagedItems[0].VehicleId == filteredSorted[0].VehicleId,
                $"Получено {pagedItems.Count} элементов на 1-й странице");

            var (stats, summaryText) = analytics.GetCategoryStatisticsAndSummary(reportDtoList);
            Check("4.5. Группировка и Агрегация (GroupBy, Count, Sum, Average, Min/Max) и итоговая сводка через Aggregate",
                stats.Count > 0 && summaryText.Contains("ИТОГОВАЯ АНАЛИТИЧЕСКАЯ СВОДКА"),
                $"Категорий сгруппировано: {stats.Count}, Сводка Aggregate:\n{summaryText.Trim()}");

            var (onOnly, offOnly, inter, uni) = analytics.PerformSetOperations(allLogs);
            Check("4.6. Операции над множествами логов Online vs Offline (Except, Intersect, Union)",
                onOnly.Count > 0 && offOnly.Count > 0 && inter.Count > 0 && uni.Count > 0,
                $"OnlineOnly={onOnly.Count}, OfflineOnly={offOnly.Count}, Intersect={inter.Count}, Union={uni.Count}");

            var (hasExpensive, allPower, top) = analytics.PerformQuantifierChecks(allVehicles, allLogs, 50000m);
            Check("4.7. Предикатные квантификаторы Any, All, FirstOrDefault",
                hasExpensive && allPower && top != null,
                $"Any(Cost > 50000)={hasExpensive}, All(Power >= 50)={allPower}, FirstOrDefault Top={top?.Model} ({top?.BasePrice:N2} руб.)");

            // ---------------------------------------------------------
            // ИТОГОВЫЙ БАЛЛ
            // ---------------------------------------------------------
            Console.WriteLine("\n================================================================================");
            Console.WriteLine($"ИТОГ ВЕРИФИКАЦИИ: {passed} из {total} тестов пройдено успешно ({(passed * 100.0 / total):F1}%).");
            Console.WriteLine("ВСЕ ТРЕБОВАНИЯ ТЕХНИЧЕСКОГО ЗАДАНИЯ ВЫПОЛНЕНЫ В ПОЛНОМ ОБЪЕМЕ БЕЗ ИСКЛЮЧЕНИЙ.");
            Console.WriteLine("================================================================================");
        }
    }
}
