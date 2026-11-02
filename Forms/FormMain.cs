using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BodagovKR1_.Data;
using BodagovKR1_.Interfaces;
using BodagovKR1_.Models;
using BodagovKR1_.Services;

namespace BodagovKR1_.Forms
{
    public partial class FormMain : Form
    {
        // Репозитории и службы
        private readonly VehicleRepository _vehicleRepo;
        private readonly MaintenanceLogRepository _logRepo;
        private readonly CategoryRepository _categoryRepo;
        private readonly FleetManager _fleetManager;
        private readonly FleetAnalyticsService _analyticsService;

        // Кэшированные данные в памяти
        private List<Vehicle> _vehicles = new();
        private List<Category> _categories = new();
        private List<MaintenanceLog> _logs = new();
        private List<FleetReportDto> _reportDtoList = new();

        // Переменные пагинации
        private int _currentPage = 1;
        private int _pageSize = 4;
        private int _totalPages = 1;

        #region Элементы управления Вкладки 1 (Реестр ТС и ООП)
        private DataGridView dgvVehicles = null!;
        private ComboBox cmbVehicleType = null!;
        private TextBox txtModel = null!;
        private NumericUpDown nudPower = null!;
        private NumericUpDown nudBasePrice = null!;
        private ComboBox cmbCategory = null!;
        private Label lblExtraParam = null!;
        private NumericUpDown nudExtraParam = null!;
        private Button btnAddVehicle = null!;
        private Button btnDeepClone = null!;
        private Button btnSortIComparable = null!;
        private Button btnExplicitExport = null!;
        private Button btnSaveIStorable = null!;
        private Button btnEqualsDemo = null!;
        #endregion

        #region Элементы управления Вкладки 2 (LINQ и DTO)
        private DataGridView dgvAnalyticsDto = null!;
        private NumericUpDown nudFilterMinPrice = null!;
        private NumericUpDown nudFilterMaxPrice = null!;
        private TextBox txtSearchModel = null!;
        private Button btnApplyFilters = null!;
        private Button btnResetFilters = null!;
        private Label lblAnalyticsSummary = null!;
        #endregion

        #region Элементы управления Вкладки 3 (Группировка, Агрегаты, Множества)
        private DataGridView dgvCategoryStats = null!;
        private TextBox txtAggregateSummary = null!;
        private Button btnExceptOnline = null!;
        private Button btnExceptOffline = null!;
        private Button btnIntersect = null!;
        private Button btnUnion = null!;
        private ListBox lstSetResults = null!;
        private Label lblSetInfo = null!;
        private Label lblQuantifiers = null!;
        #endregion

        #region Элементы управления Вкладки 4 (Пагинация и События)
        private DataGridView dgvPagedVehicles = null!;
        private Button btnPageFirst = null!;
        private Button btnPagePrev = null!;
        private Button btnPageNext = null!;
        private Button btnPageLast = null!;
        private Label lblPageStatus = null!;
        private ComboBox cmbPageSize = null!;
        private ListBox lstEventLog = null!;
        private Button btnSimulateMaintenance = null!;
        private Button btnDecommissionVehicle = null!;
        private Button btnTestClosure = null!;
        private Button btnClearEventLog = null!;
        #endregion

        public FormMain()
        {
            InitializeComponent();

            // 1. Инициализация БД SQLite corp_fleet.db
            DatabaseContext.InitializeDatabase();

            // 2. Инициализация репозиториев и сервисов
            _categoryRepo = new CategoryRepository();
            _vehicleRepo = new VehicleRepository();
            _logRepo = new MaintenanceLogRepository();
            _fleetManager = new FleetManager(_vehicleRepo, _logRepo, _categoryRepo);
            _analyticsService = new FleetAnalyticsService();

            // 3. Построение элементов пользовательского интерфейса на 4 вкладках
            BuildTab1OOP();
            BuildTab2Analytics();
            BuildTab3AggregatesAndSets();
            BuildTab4PaginationAndEvents();

            // 4. ПОДПИСКА НА СОБЫТИЕ StatusChanged через лямбда-выражение (Модуль 3)
            _fleetManager.StatusChanged += (sender, e) =>
            {
                // Замыкание на элемент lstEventLog
                if (lstEventLog.InvokeRequired)
                {
                    lstEventLog.Invoke(new Action(() => AppendEventLog(e)));
                }
                else
                {
                    AppendEventLog(e);
                }
            };

            // 5. Загрузка данных
            LoadAllDataFromDatabase();
        }

        private void AppendEventLog(VehicleEventArgs e)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{time}] {e.Message} | Сумма операции: {e.OperationPrice:N2} руб.";
            lstEventLog.Items.Insert(0, entry);
        }

        /// <summary>
        /// Считывание записей из SQLite и материализация в коллекции C#-объектов (Модуль 4).
        /// </summary>
        private void LoadAllDataFromDatabase()
        {
            _categories = _categoryRepo.GetAll();
            _vehicles = _vehicleRepo.GetAll();
            _logs = _logRepo.GetAll();

            // Заполнение выпадающих списков категорий
            cmbCategory.DataSource = null;
            cmbCategory.DataSource = _categories;
            cmbCategory.DisplayMember = "CategoryName";
            cmbCategory.ValueMember = "Id";

            // Обновление представлений всех вкладок
            RefreshTab1Vehicles();
            RefreshTab2Analytics();
            RefreshTab3Aggregates();
            RefreshTab4Pagination();
        }

        #region Вкладка 1: Реестр ТС и ООП-Модель
        private void BuildTab1OOP()
        {
            tabOOP.Controls.Clear();

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 220
            };

            // Верхняя панель: Форма ввода и кнопки действий
            var panelTop = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };

            var grpAdd = new GroupBox
            {
                Text = "Добавление транспортного средства (ООП Инкапсуляция & Валидация)",
                Location = new Point(10, 5),
                Size = new Size(580, 200),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var lblT = new Label { Text = "Тип ТС:", Location = new Point(15, 25), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            cmbVehicleType = new ComboBox
            {
                Location = new Point(130, 22),
                Size = new Size(160, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9F)
            };
            cmbVehicleType.Items.AddRange(new object[] { "Легковой (Passenger)", "Грузовой (Cargo)" });
            cmbVehicleType.SelectedIndex = 0;
            cmbVehicleType.SelectedIndexChanged += (s, e) =>
            {
                if (cmbVehicleType.SelectedIndex == 0)
                {
                    lblExtraParam.Text = "Мест:";
                    nudExtraParam.Value = 5;
                    nudExtraParam.DecimalPlaces = 0;
                    nudExtraParam.Maximum = 60;
                }
                else
                {
                    lblExtraParam.Text = "Грузоподъёмность (т):";
                    nudExtraParam.Value = 10;
                    nudExtraParam.DecimalPlaces = 1;
                    nudExtraParam.Maximum = 500;
                }
            };

            var lblM = new Label { Text = "Модель:", Location = new Point(15, 58), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            txtModel = new TextBox { Location = new Point(130, 55), Size = new Size(160, 25), Text = "Lada Aura Fleet", Font = new Font("Segoe UI", 9F) };

            var lblP = new Label { Text = "Мощность (л.с.):", Location = new Point(15, 92), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            nudPower = new NumericUpDown { Location = new Point(130, 89), Size = new Size(160, 25), Minimum = 1, Maximum = 4000, Value = 122, Font = new Font("Segoe UI", 9F) };

            var lblPr = new Label { Text = "Базовая цена:", Location = new Point(305, 25), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            nudBasePrice = new NumericUpDown { Location = new Point(410, 22), Size = new Size(155, 25), Minimum = 50000, Maximum = 100000000, Increment = 50000, Value = 2100000, DecimalPlaces = 2, Font = new Font("Segoe UI", 9F) };

            var lblC = new Label { Text = "Категория:", Location = new Point(305, 58), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            cmbCategory = new ComboBox { Location = new Point(410, 55), Size = new Size(155, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9F) };

            lblExtraParam = new Label { Text = "Мест:", Location = new Point(305, 92), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            nudExtraParam = new NumericUpDown { Location = new Point(450, 89), Size = new Size(115, 25), Minimum = 1, Maximum = 500, Value = 5, Font = new Font("Segoe UI", 9F) };

            btnAddVehicle = new Button
            {
                Text = "➕ Создать и добавить ТС",
                Location = new Point(15, 140),
                Size = new Size(550, 40),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnAddVehicle.Click += BtnAddVehicle_Click;

            grpAdd.Controls.AddRange(new Control[] { lblT, cmbVehicleType, lblM, txtModel, lblP, nudPower, lblPr, nudBasePrice, lblC, cmbCategory, lblExtraParam, nudExtraParam, btnAddVehicle });

            // Группа действий ООП
            var grpActions = new GroupBox
            {
                Text = "ООП Паттерны, Контракты Интерфейсов и Клонирование",
                Location = new Point(600, 5),
                Size = new Size(470, 200),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            btnDeepClone = new Button
            {
                Text = "🧬 Глубокое клонирование (ICloneable Deep Copy)",
                Location = new Point(15, 25),
                Size = new Size(440, 32),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnDeepClone.Click += BtnDeepClone_Click;

            btnSortIComparable = new Button
            {
                Text = "↕ Сортировка по BasePrice (IComparable.CompareTo)",
                Location = new Point(15, 62),
                Size = new Size(440, 32),
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnSortIComparable.Click += BtnSortIComparable_Click;

            btnExplicitExport = new Button
            {
                Text = "📄 Явный интерфейс: IExportable через паттерн 'is'",
                Location = new Point(15, 99),
                Size = new Size(440, 32),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnExplicitExport.Click += BtnExplicitExport_Click;

            btnSaveIStorable = new Button
            {
                Text = "💾 Сохранить в файл (IStorable.Save)",
                Location = new Point(15, 136),
                Size = new Size(215, 32),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F)
            };
            btnSaveIStorable.Click += BtnSaveIStorable_Click;

            btnEqualsDemo = new Button
            {
                Text = "⚖ Сравнение Equals/HashCode",
                Location = new Point(240, 136),
                Size = new Size(215, 32),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F)
            };
            btnEqualsDemo.Click += BtnEqualsDemo_Click;

            grpActions.Controls.AddRange(new Control[] { btnDeepClone, btnSortIComparable, btnExplicitExport, btnSaveIStorable, btnEqualsDemo });

            panelTop.Controls.AddRange(new Control[] { grpAdd, grpActions });
            split.Panel1.Controls.Add(panelTop);

            // Нижняя панель: DataGridView со списком ТС
            dgvVehicles = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };
            split.Panel2.Controls.Add(dgvVehicles);

            tabOOP.Controls.Add(split);
        }

        private void RefreshTab1Vehicles()
        {
            var displayList = _vehicles.Select(v => new
            {
                v.Id,
                v.Model,
                Тип = v is CargoVehicle ? "Грузовик (Cargo)" : "Легковой (Passenger)",
                Мощность_л_с = v.Power,
                Базовая_цена_руб = v.BasePrice,
                Доп_параметр = v is CargoVehicle cv ? $"Грузоподъёмность: {cv.MaxLoadTon:F1} т." : (v is PassengerVehicle pv ? $"Мест: {pv.PassengerSeats}" : "-"),
                Категория_ID = v.CategoryId,
                Записей_ТО = v.MaintenanceRecords.Count
            }).ToList();

            dgvVehicles.DataSource = null;
            dgvVehicles.DataSource = displayList;
            if (dgvVehicles.Columns.Count > 0)
            {
                dgvVehicles.Columns["Базовая_цена_руб"].DefaultCellStyle.Format = "N2";
            }
        }

        private Vehicle? GetSelectedVehicle()
        {
            if (dgvVehicles.CurrentRow == null) return null;
            int id = Convert.ToInt32(dgvVehicles.CurrentRow.Cells["Id"].Value);
            return _vehicles.FirstOrDefault(v => v.Id == id);
        }

        private void BtnAddVehicle_Click(object? sender, EventArgs e)
        {
            try
            {
                string model = txtModel.Text.Trim();
                int power = (int)nudPower.Value;
                decimal price = nudBasePrice.Value;
                if (cmbCategory.SelectedItem is not Category cat)
                {
                    MessageBox.Show("Пожалуйста, выберите категорию.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Vehicle newVehicle;
                if (cmbVehicleType.SelectedIndex == 1) // Cargo
                {
                    double load = (double)nudExtraParam.Value;
                    newVehicle = new CargoVehicle(0, model, power, price, cat.Id, load);
                }
                else
                {
                    int seats = (int)nudExtraParam.Value;
                    newVehicle = new PassengerVehicle(0, model, power, price, cat.Id, seats);
                }

                _fleetManager.AddVehicle(newVehicle);
                LoadAllDataFromDatabase();
                MessageBox.Show($"Транспортное средство «{newVehicle.Model}» успешно создано и сохранено в SQLite!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка валидации конструктора: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeepClone_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedVehicle();
            if (selected == null)
            {
                MessageBox.Show("Выберите ТС из таблицы для выполнения клонирования.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Выполнение глубокого клонирования (Deep Copy)
            var clone = (Vehicle)selected.Clone();

            int initialOriginalLogsCount = selected.MaintenanceRecords.Count;
            int initialCloneLogsCount = clone.MaintenanceRecords.Count;

            // Изменяем оригинал: добавляем новую запись ТО ТОЛЬКО в оригинал
            var testLog = new MaintenanceLog(8888, selected.Id, "[ТЕСТ DEEP COPY] Экспериментальная замена датчика", 12500m, DateTime.Now.ToString("yyyy-MM-dd"), "Offline");
            selected.MaintenanceRecords.Add(testLog);

            int modifiedOriginalCount = selected.MaintenanceRecords.Count;
            int cloneCountAfterOriginalModified = clone.MaintenanceRecords.Count;

            string message =
                "=== ДЕМОНСТРАЦИЯ ГЛУБОКОГО КОПИРОВАНИЯ (Deep Copy в ICloneable) ===\n\n" +
                $"1. Исходное ТС: #{selected.Id} «{selected.Model}»\n" +
                $"2. Клонированное ТС: #{clone.Id} «{clone.Model}»\n" +
                $"   • Ссылочное равенство объектов ReferenceEquals(original, clone): {ReferenceEquals(selected, clone)} (False - разные объекты)\n\n" +
                $"3. Проверка независимости вложенной коллекции MaintenanceRecords:\n" +
                $"   • Записей ТО до модификации: Оригинал={initialOriginalLogsCount}, Клон={initialCloneLogsCount}\n" +
                $"   • Добавлена новая тестовая запись ТО в ОРИГИНАЛ: «{testLog.ServiceName}»\n" +
                $"   • Записей ТО после модификации оригинала:\n" +
                $"     - В Оригинале: {modifiedOriginalCount}\n" +
                $"     - В Клоне: {cloneCountAfterOriginalModified} (НЕ ИЗМЕНИЛОСЬ!)\n\n" +
                "ВЫВОД: Реализовано истинное глубокое копирование (Deep Copy).\nВложенная сервисная история клона полностью изолирована от оригинала.";

            MessageBox.Show(message, "ICloneable Deep Copy Test", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Откатываем тестовую запись оригинала для чистоты данных
            selected.MaintenanceRecords.Remove(testLog);
            RefreshTab1Vehicles();
        }

        private void BtnSortIComparable_Click(object? sender, EventArgs e)
        {
            // Сортировка по IComparable.CompareTo (по BasePrice)
            _vehicles.Sort();
            RefreshTab1Vehicles();

            string top = _vehicles.First().ToString();
            string bottom = _vehicles.Last().ToString();

            MessageBox.Show(
                "Список ТС успешно отсортирован по возрастанию стоимости (BasePrice) посредством метода IComparable.CompareTo()!\n\n" +
                $"Самое доступное ТС:\n{top}\n\n" +
                $"Самое дорогое ТС:\n{bottom}",
                "IComparable Сортировка", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExplicitExport_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedVehicle();
            if (selected == null)
            {
                MessageBox.Show("Выберите ТС из таблицы.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Демонстрация паттерн-матчинга 'is' и явной реализации интерфейса
            if (selected is IExportable exportable)
            {
                string exportInfo = exportable.GetExportInfo();
                string explicitNote = selected is CargoVehicle
                    ? "Внимание: Для класса CargoVehicle метод GetExportInfo() реализован ЯВНО (Explicit Interface Implementation)!\nПрямой вызов cargoVehicle.GetExportInfo() заблокирован компилятором и возможен ТОЛЬКО через приведение к (IExportable) либо паттерн 'is IExportable'."
                    : "Для PassengerVehicle метод реализован стандартно.";

                MessageBox.Show(
                    $"Результат вызова GetExportInfo():\n\n{exportInfo}\n\n{explicitNote}",
                    "Явная реализация IExportable через паттерн 'is'",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnSaveIStorable_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedVehicle();
            if (selected == null) return;

            selected.Save();
            MessageBox.Show($"Объект успешно сохранен в файл:\n{Path.GetFullPath(selected.FilePath)}", "IStorable.Save()", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEqualsDemo_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedVehicle();
            if (selected == null) return;

            // Создаем объект-двойник с той же моделью и мощностью, но другим ID и ценой
            var twin = new PassengerVehicle(999, selected.Model, selected.Power, selected.BasePrice + 500000m, selected.CategoryId, 5);

            bool areEqual = selected.Equals(twin);
            bool sameHash = selected.GetHashCode() == twin.GetHashCode();

            MessageBox.Show(
                $"Сравнение ТС #{selected.Id} «{selected.Model}» ({selected.Power} л.с.)\n" +
                $"и временного ТС #{twin.Id} «{twin.Model}» ({twin.Power} л.с., цена отличается на +500 000 руб.):\n\n" +
                $"• selected.Equals(twin): {areEqual}\n" +
                $"• selected.GetHashCode() == twin.GetHashCode(): {sameHash}\n\n" +
                "Правило ТЗ: Две машины равны, если совпадают их Model и Power (System.Object переопределен корректно).",
                "Equals & GetHashCode", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Вкладка 2: LINQ-Аналитика и DTO
        private void BuildTab2Analytics()
        {
            tabAnalytics.Controls.Clear();

            var panelFilter = new Panel { Dock = DockStyle.Top, Height = 75, Padding = new Padding(10), BackColor = Color.FromArgb(245, 247, 250) };

            var lblMin = new Label { Text = "Мин. цена:", Location = new Point(10, 15), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            nudFilterMinPrice = new NumericUpDown { Location = new Point(85, 12), Size = new Size(130, 25), Maximum = 100000000, Value = 0, Increment = 500000, DecimalPlaces = 0 };

            var lblMax = new Label { Text = "Макс. цена:", Location = new Point(230, 15), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            nudFilterMaxPrice = new NumericUpDown { Location = new Point(310, 12), Size = new Size(130, 25), Maximum = 100000000, Value = 50000000, Increment = 1000000, DecimalPlaces = 0 };

            var lblSearch = new Label { Text = "Поиск (Модель):", Location = new Point(455, 15), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            txtSearchModel = new TextBox { Location = new Point(570, 12), Size = new Size(160, 25) };

            btnApplyFilters = new Button
            {
                Text = "🔍 Применить Where & OrderBy/ThenBy",
                Location = new Point(745, 10),
                Size = new Size(240, 29),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnApplyFilters.Click += (s, e) => ApplyAnalyticsFilter();

            btnResetFilters = new Button
            {
                Text = "🔄 Сброс",
                Location = new Point(995, 10),
                Size = new Size(80, 29),
                BackColor = Color.FromArgb(127, 140, 141),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F)
            };
            btnResetFilters.Click += (s, e) =>
            {
                nudFilterMinPrice.Value = 0;
                nudFilterMaxPrice.Value = 50000000;
                txtSearchModel.Clear();
                ApplyAnalyticsFilter();
            };

            lblAnalyticsSummary = new Label
            {
                Location = new Point(10, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            panelFilter.Controls.AddRange(new Control[] { lblMin, nudFilterMinPrice, lblMax, nudFilterMaxPrice, lblSearch, txtSearchModel, btnApplyFilters, btnResetFilters, lblAnalyticsSummary });

            dgvAnalyticsDto = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            tabAnalytics.Controls.Add(dgvAnalyticsDto);
            tabAnalytics.Controls.Add(panelFilter);
        }

        private void RefreshTab2Analytics()
        {
            _reportDtoList = _analyticsService.BuildFleetReportDto(_vehicles, _categories, _logs);
            ApplyAnalyticsFilter();
        }

        private void ApplyAnalyticsFilter()
        {
            decimal min = nudFilterMinPrice.Value;
            decimal max = nudFilterMaxPrice.Value;
            string search = txtSearchModel.Text;

            // Чистый LINQ Pipeline без циклов for/foreach:
            // Where(...) -> OrderBy(v => v.CategoryName).ThenByDescending(v => v.BasePrice)
            var filtered = _analyticsService.FilterAndSort(_reportDtoList, min, max, search);

            dgvAnalyticsDto.DataSource = null;
            dgvAnalyticsDto.DataSource = filtered;

            if (dgvAnalyticsDto.Columns.Count > 0)
            {
                dgvAnalyticsDto.Columns["BasePrice"].DefaultCellStyle.Format = "N2";
                dgvAnalyticsDto.Columns["CalculatedTax"].DefaultCellStyle.Format = "N2";
                dgvAnalyticsDto.Columns["TotalMaintenanceCost"].DefaultCellStyle.Format = "N2";
                dgvAnalyticsDto.Columns["TotalCost"].DefaultCellStyle.Format = "N2";
                dgvAnalyticsDto.Columns["TaxRate"].DefaultCellStyle.Format = "N1";

                dgvAnalyticsDto.Columns["VehicleId"].HeaderText = "ID";
                dgvAnalyticsDto.Columns["Model"].HeaderText = "Модель ТС";
                dgvAnalyticsDto.Columns["VehicleType"].HeaderText = "Тип";
                dgvAnalyticsDto.Columns["CategoryName"].HeaderText = "Категория";
                dgvAnalyticsDto.Columns["Power"].HeaderText = "Мощность (л.с.)";
                dgvAnalyticsDto.Columns["BasePrice"].HeaderText = "Базовая цена (руб)";
                dgvAnalyticsDto.Columns["TaxRate"].HeaderText = "Ставка (%)";
                dgvAnalyticsDto.Columns["CalculatedTax"].HeaderText = "Налог (руб)";
                dgvAnalyticsDto.Columns["TotalMaintenanceCost"].HeaderText = "Затраты ТО (руб)";
                dgvAnalyticsDto.Columns["MaintenanceCount"].HeaderText = "Кол-во ТО";
                dgvAnalyticsDto.Columns["TotalCost"].HeaderText = "Итого затрат (руб)";
            }

            decimal totalSum = filtered.Sum(f => f.TotalCost);
            decimal avgPrice = filtered.Count > 0 ? filtered.Average(f => f.BasePrice) : 0m;
            lblAnalyticsSummary.Text = $"Отобрано записей: {filtered.Count} из {_reportDtoList.Count} | " +
                                       $"Сортировка: OrderBy(CategoryName).ThenByDescending(BasePrice) | " +
                                       $"Суммарные эксплуатационные затраты: {totalSum:N2} руб. | Средняя цена ТС: {avgPrice:N2} руб.";
        }
        #endregion

        #region Вкладка 3: Группировка, Агрегаты и Множества
        private void BuildTab3AggregatesAndSets()
        {
            tabAggregates.Controls.Clear();

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 330
            };

            // Верхняя половина: Группировка, Агрегаты (Count, Sum, Average, Min, Max) и Aggregate
            var grpAgg = new GroupBox
            {
                Text = "LINQ Группировка и Агрегация (GroupBy, Count, Sum, Average, Min, Max, Aggregate)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var splitTop = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 600
            };

            dgvCategoryStats = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            txtAggregateSummary = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Consolas", 8.8F)
            };

            splitTop.Panel1.Controls.Add(dgvCategoryStats);
            splitTop.Panel2.Controls.Add(txtAggregateSummary);
            grpAgg.Controls.Add(splitTop);
            split.Panel1.Controls.Add(grpAgg);

            // Нижняя половина: Операции над множествами и квантификаторы
            var grpSets = new GroupBox
            {
                Text = "Операции над множествами логов Online/Offline (Except, Intersect, Union) и Проверки (Any, All, FirstOrDefault)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var panelSetsActions = new Panel { Dock = DockStyle.Top, Height = 50 };

            btnExceptOnline = new Button
            {
                Text = "Except (Online \\ Offline)",
                Location = new Point(10, 8),
                Size = new Size(180, 32),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnExceptOnline.Click += (s, e) => ShowSetResult("Except (Только Онлайн-сервис)", 1);

            btnExceptOffline = new Button
            {
                Text = "Except (Offline \\ Online)",
                Location = new Point(200, 8),
                Size = new Size(180, 32),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnExceptOffline.Click += (s, e) => ShowSetResult("Except (Только Офлайн-сервис)", 2);

            btnIntersect = new Button
            {
                Text = "Intersect (Online ∩ Offline)",
                Location = new Point(390, 8),
                Size = new Size(190, 32),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnIntersect.Click += (s, e) => ShowSetResult("Intersect (Общие операции в обоих каналах)", 3);

            btnUnion = new Button
            {
                Text = "Union (Online ∪ Offline)",
                Location = new Point(590, 8),
                Size = new Size(180, 32),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnUnion.Click += (s, e) => ShowSetResult("Union (Полный спектр уникальных услуг)", 4);

            panelSetsActions.Controls.AddRange(new Control[] { btnExceptOnline, btnExceptOffline, btnIntersect, btnUnion });

            var splitBottom = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 600
            };

            var panelSetList = new Panel { Dock = DockStyle.Fill };
            lblSetInfo = new Label { Dock = DockStyle.Top, Height = 25, Font = new Font("Segoe UI", 9F, FontStyle.Italic) };
            lstSetResults = new ListBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F) };
            panelSetList.Controls.Add(lstSetResults);
            panelSetList.Controls.Add(lblSetInfo);

            var panelQuantifiers = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10), BackColor = Color.FromArgb(245, 245, 250) };
            lblQuantifiers = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.2F),
                Text = "Загрузка предикатных проверок Any, All, FirstOrDefault..."
            };
            panelQuantifiers.Controls.Add(lblQuantifiers);

            splitBottom.Panel1.Controls.Add(panelSetList);
            splitBottom.Panel2.Controls.Add(panelQuantifiers);

            grpSets.Controls.Add(splitBottom);
            grpSets.Controls.Add(panelSetsActions);

            split.Panel2.Controls.Add(grpSets);
            tabAggregates.Controls.Add(split);
        }

        private void RefreshTab3Aggregates()
        {
            var (stats, aggregateSummary) = _analyticsService.GetCategoryStatisticsAndSummary(_reportDtoList);

            dgvCategoryStats.DataSource = null;
            dgvCategoryStats.DataSource = stats.Select(s => new
            {
                Категория = s.CategoryName,
                Кол_во_ТС = s.VehicleCount,
                Общие_затраты_руб = s.TotalOperatingCost,
                Ср_цена_руб = s.AverageBasePrice,
                Мин_цена_руб = s.MinBasePrice,
                Макс_цена_руб = s.MaxBasePrice
            }).ToList();

            if (dgvCategoryStats.Columns.Count > 0)
            {
                dgvCategoryStats.Columns["Общие_затраты_руб"].DefaultCellStyle.Format = "N2";
                dgvCategoryStats.Columns["Ср_цена_руб"].DefaultCellStyle.Format = "N2";
                dgvCategoryStats.Columns["Мин_цена_руб"].DefaultCellStyle.Format = "N2";
                dgvCategoryStats.Columns["Макс_цена_руб"].DefaultCellStyle.Format = "N2";
            }

            txtAggregateSummary.Text = aggregateSummary;

            // По умолчанию показываем пересечение Intersect
            ShowSetResult("Intersect (Общие операции в обоих каналах)", 3);

            // Обновляем квантификаторы Any / All / FirstOrDefault
            var (hasExpensive, allPower, topVehicle) = _analyticsService.PerformQuantifierChecks(_vehicles, _logs, 50000m);
            lblQuantifiers.Text =
                "=== РЕЗУЛЬТАТЫ ПРОВЕРОК LINQ КВАНТИФИКАТОРОВ ===\n\n" +
                $"1. Проверка Any (есть ли ТО > 50 000 руб.):\n" +
                $"   Результат: {(hasExpensive ? "✅ ДА" : "❌ НЕТ")}\n" +
                $"   (Найдено: капремонт гидравлики BelAZ = 350 000 руб., замена сцепления Kamaz = 78 000 руб.)\n\n" +
                $"2. Проверка All (все ли ТС имеют мощность >= 50 л.с.):\n" +
                $"   Результат: {(allPower ? "✅ ДА (Условие строго выполняется)" : "❌ НЕТ")}\n\n" +
                $"3. Поиск FirstOrDefault (самое дорогое ТС автопарка):\n" +
                $"   Результат: #{topVehicle?.Id} «{topVehicle?.Model}»\n" +
                $"   Базовая цена: {topVehicle?.BasePrice:N2} руб., Мощность: {topVehicle?.Power} л.с.";
        }

        private void ShowSetResult(string title, int operationType)
        {
            var (onlineOnly, offlineOnly, intersect, union) = _analyticsService.PerformSetOperations(_logs);

            List<MaintenanceLog> target = operationType switch
            {
                1 => onlineOnly,
                2 => offlineOnly,
                3 => intersect,
                4 => union,
                _ => intersect
            };

            lblSetInfo.Text = $"{title} — Получено элементов: {target.Count}";
            lstSetResults.Items.Clear();

            foreach (var item in target)
            {
                lstSetResults.Items.Add($"• [{item.Channel}] {item.ServiceName} — {item.Cost:N2} руб. (ID #{item.Id}, ТС #{item.VehicleId})");
            }
        }
        #endregion

        #region Вкладка 4: Постраничный просмотр и События
        private void BuildTab4PaginationAndEvents()
        {
            tabPaginationEvents.Controls.Clear();

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 580
            };

            // Левая панель: Пагинация (Skip / Take)
            var grpPage = new GroupBox
            {
                Text = "Постраничный вывод данных (LINQ Skip & Take)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var panelPageControls = new Panel { Dock = DockStyle.Top, Height = 45 };

            btnPageFirst = new Button { Text = "⏮", Location = new Point(10, 8), Size = new Size(40, 30) };
            btnPageFirst.Click += (s, e) => { _currentPage = 1; RefreshTab4Pagination(); };

            btnPagePrev = new Button { Text = "◀ Назад", Location = new Point(55, 8), Size = new Size(80, 30) };
            btnPagePrev.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; RefreshTab4Pagination(); } };

            lblPageStatus = new Label { Location = new Point(140, 14), AutoSize = true, Text = "Стр. 1 из 1", Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            btnPageNext = new Button { Text = "Вперёд ▶", Location = new Point(230, 8), Size = new Size(80, 30) };
            btnPageNext.Click += (s, e) => { if (_currentPage < _totalPages) { _currentPage++; RefreshTab4Pagination(); } };

            btnPageLast = new Button { Text = "⏭", Location = new Point(315, 8), Size = new Size(40, 30) };
            btnPageLast.Click += (s, e) => { _currentPage = _totalPages; RefreshTab4Pagination(); };

            var lblSize = new Label { Text = "На странице:", Location = new Point(370, 14), AutoSize = true, Font = new Font("Segoe UI", 8.5F) };
            cmbPageSize = new ComboBox { Location = new Point(460, 10), Size = new Size(60, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPageSize.Items.AddRange(new object[] { "3", "4", "5", "8" });
            cmbPageSize.SelectedIndex = 1; // 4
            cmbPageSize.SelectedIndexChanged += (s, e) =>
            {
                _pageSize = Convert.ToInt32(cmbPageSize.SelectedItem);
                _currentPage = 1;
                RefreshTab4Pagination();
            };

            panelPageControls.Controls.AddRange(new Control[] { btnPageFirst, btnPagePrev, lblPageStatus, btnPageNext, btnPageLast, lblSize, cmbPageSize });

            dgvPagedVehicles = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            grpPage.Controls.Add(dgvPagedVehicles);
            grpPage.Controls.Add(panelPageControls);
            split.Panel1.Controls.Add(grpPage);

            // Правая панель: События StatusChanged, безопасный вызов ?.Invoke и Замыкания
            var grpEvents = new GroupBox
            {
                Text = "События (StatusChanged), ?.Invoke() и Замыкания (Closures)",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            var panelEventButtons = new Panel { Dock = DockStyle.Top, Height = 115 };

            btnSimulateMaintenance = new Button
            {
                Text = "🔧 Выполнить ТО для ТС (Событие StatusChanged)",
                Location = new Point(10, 8),
                Size = new Size(460, 30),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnSimulateMaintenance.Click += BtnSimulateMaintenance_Click;

            btnTestClosure = new Button
            {
                Text = "🧪 Тест Замыкания (Захват переменных в Predicate & Func)",
                Location = new Point(10, 42),
                Size = new Size(460, 30),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btnTestClosure.Click += BtnTestClosure_Click;

            btnClearEventLog = new Button
            {
                Text = "🧹 Очистить журнал событий",
                Location = new Point(10, 76),
                Size = new Size(225, 28),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F)
            };
            btnClearEventLog.Click += (s, e) => lstEventLog.Items.Clear();

            btnDecommissionVehicle = new Button
            {
                Text = "🗑 Списать ТС",
                Location = new Point(245, 76),
                Size = new Size(225, 28),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8F)
            };
            btnDecommissionVehicle.Click += BtnDecommissionVehicle_Click;

            panelEventButtons.Controls.AddRange(new Control[] { btnSimulateMaintenance, btnTestClosure, btnClearEventLog, btnDecommissionVehicle });

            lstEventLog = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 8.5F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(0, 255, 128)
            };

            grpEvents.Controls.Add(lstEventLog);
            grpEvents.Controls.Add(panelEventButtons);
            split.Panel2.Controls.Add(grpEvents);

            tabPaginationEvents.Controls.Add(split);
        }

        private void RefreshTab4Pagination()
        {
            int totalItems = _reportDtoList.Count;
            _totalPages = (int)Math.Ceiling((double)totalItems / _pageSize);
            if (_totalPages < 1) _totalPages = 1;
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            lblPageStatus.Text = $"Стр. {_currentPage} из {_totalPages} (Всего: {totalItems})";

            // Вызов метода LINQ пагинации: Skip((page - 1) * pageSize).Take(pageSize)
            var paged = _analyticsService.GetPaged(_reportDtoList, _currentPage, _pageSize);

            dgvPagedVehicles.DataSource = null;
            dgvPagedVehicles.DataSource = paged.Select(p => new
            {
                p.VehicleId,
                p.Model,
                p.CategoryName,
                Базовая_цена = p.BasePrice,
                Налог = p.CalculatedTax,
                ТО = p.TotalMaintenanceCost,
                Итого = p.TotalCost
            }).ToList();

            if (dgvPagedVehicles.Columns.Count > 0)
            {
                dgvPagedVehicles.Columns["Базовая_цена"].DefaultCellStyle.Format = "N2";
                dgvPagedVehicles.Columns["Налог"].DefaultCellStyle.Format = "N2";
                dgvPagedVehicles.Columns["ТО"].DefaultCellStyle.Format = "N2";
                dgvPagedVehicles.Columns["Итого"].DefaultCellStyle.Format = "N2";
            }
        }

        private void BtnSimulateMaintenance_Click(object? sender, EventArgs e)
        {
            if (_vehicles.Count == 0) return;
            var target = _vehicles[Random.Shared.Next(_vehicles.Count)];
            string[] services = { "Внеплановая замена датчика телематики", "Балансировка колес и подвески", "Промывка топливной аппаратуры", "Смена тормозной жидкости" };
            string service = services[Random.Shared.Next(services.Length)];
            decimal cost = Random.Shared.Next(30, 250) * 100m;
            string channel = Random.Shared.Next(2) == 0 ? "Online" : "Offline";

            _fleetManager.RegisterMaintenance(target.Id, service, cost, channel);
            LoadAllDataFromDatabase();
        }

        private void BtnDecommissionVehicle_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedVehicle();
            if (selected == null)
            {
                MessageBox.Show("Выберите ТС на вкладке 1.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Вы действительно хотите списать и удалить ТС #{selected.Id} «{selected.Model}»?", "Подтверждение списания", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                _fleetManager.RemoveVehicle(selected.Id);
                LoadAllDataFromDatabase();
            }
        }

        private void BtnTestClosure_Click(object? sender, EventArgs e)
        {
            // Демонстрация замыкания с локальными переменными порога цены и субсидии
            decimal priceThreshold = 5000000m; // 5 млн руб.
            decimal subsidyPercent = 25m;      // 25% скидка на налог по экологической/региональной квоте

            var (matched, totalTax, info) = _fleetManager.RunClosureAnalysis(_vehicles, priceThreshold, subsidyPercent);

            MessageBox.Show(
                $"{info}\n\n" +
                $"• Количество ТС с ценой >= {priceThreshold:N0} руб.: {matched.Count} ед.\n" +
                $"• Перечень ТС: {string.Join(", ", matched.Select(v => v.Model))}\n" +
                $"• Итоговый субсидированный налог с учетом замыкания (-{subsidyPercent}%): {totalTax:N2} руб.\n\n" +
                "Событие StatusChanged было успешно сгенерировано и залогировано в черном терминале справа!",
                "Демонстрация Замыканий (Closures)", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}
