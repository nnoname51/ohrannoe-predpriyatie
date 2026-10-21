using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private int activeAlertId = 3;
        private int activeObjectId = 101;

        // Элементы интерфейса пульта ЧОП
        private Label lblAlertStatus;
        private Panel pnlEmergencyBanner;
        private Button btnSimulateAlert;
        private Button btnAcceptAlert;
        private Button btnAssignGBR;
        private Button btnFalseAlarm;
        private DataGridView dgvAlerts;
        private DataGridView dgvGbrTeams;

        // Хранилища данных в оперативной памяти (взамен таблиц Access)
        private DataTable alertTable;
        private DataTable gbrTable;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents(); // Генерация кнопок, таблиц и структуры
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDataTables();
        }

        // Заполнение таблиц демонстрационными данными по вашей UML-модели
        private void LoadDataTables()
        {
            try
            {
                // 1. Инициализация и заполнение журнала тревог (Таблица [Вызовы])
                alertTable = new DataTable();
                alertTable.Columns.Add("ID Вызова");
                alertTable.Columns.Add("ID Объекта");
                alertTable.Columns.Add("Дата и время");
                alertTable.Columns.Add("Статус тревоги");

                alertTable.Rows.Add("1", "104", DateTime.Now.AddMinutes(-45).ToString("HH:mm:ss"), "Архив: Ложный вызов");
                alertTable.Rows.Add("2", "108", DateTime.Now.AddMinutes(-20).ToString("HH:mm:ss"), "Завершено (Выезд ГБР)");
                dgvAlerts.DataSource = alertTable;

                // 2. Инициализация и заполнение списка экипажей (Таблица [Экипажи])
                gbrTable = new DataTable();
                gbrTable.Columns.Add("ID Экипажа");
                gbrTable.Columns.Add("Название Экипажа");
                gbrTable.Columns.Add("Зона патрулирования");
                gbrTable.Columns.Add("Статус");

                gbrTable.Rows.Add("1", "Экипаж ГБР-ЦЕНТР", "Центральный р-н", "Свободен");
                gbrTable.Rows.Add("2", "Экипаж ГБР-ЗАПАД", "Западный промузел", "Свободен");
                gbrTable.Rows.Add("3", "Экипаж ГБР-ВОСТОК", "Восточный мкр-н", "На вызове");
                dgvGbrTeams.DataSource = gbrTable;

                ResetEmergencyBanner();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации таблиц: {ex.Message}");
            }
        }

        // 1. Симуляция: Кнопка "ТЕСТ: Сработка датчика" (Генерация тревоги из Use Case)
        private void btnSimulateAlert_Click(object sender, EventArgs e)
        {
            pnlEmergencyBanner.BackColor = Color.DarkRed;
            lblAlertStatus.Text = $"КРИТИЧЕСКИЙ СИГНАЛ: Сработка датчика на объекте №{activeObjectId}!";
            lblAlertStatus.ForeColor = Color.White;

            btnAcceptAlert.Enabled = true;
            btnAssignGBR.Enabled = false;
            btnFalseAlarm.Enabled = false;

            // Добавляем новую тревогу прямо в таблицу на экране
            if (alertTable == null)
            {
                alertTable = new DataTable();
                alertTable.Columns.Add("ID Вызова");
                alertTable.Columns.Add("ID Объекта");
                alertTable.Columns.Add("Дата и время");
                alertTable.Columns.Add("Статус тревоги");
                dgvAlerts.DataSource = alertTable;
            }
            alertTable.Rows.Add(activeAlertId.ToString(), activeObjectId.ToString(), DateTime.Now.ToString("HH:mm:ss"), "Поступила");

        }

        // 2. Кнопка: Принять тревогу диспетчером
        private void btnAcceptAlert_Click(object sender, EventArgs e)
        {
            pnlEmergencyBanner.BackColor = Color.Orange;
            lblAlertStatus.Text = $"Обработка тревоги №{activeAlertId}. Изучение карточки объекта...";
            lblAlertStatus.ForeColor = Color.Black;

            btnAcceptAlert.Enabled = false;
            btnAssignGBR.Enabled = true;
            btnFalseAlarm.Enabled = true;

            // Обновляем статус в нашей таблице на экране
            if (alertTable.Rows.Count > 0)
            {
                alertTable.Rows[alertTable.Rows.Count - 1]["Статус тревоги"] = "В обработке";
            }

            MessageBox.Show($"Диспетчер изучает данные объекта №{activeObjectId}.\nПроверка договора ЧОП и подключение к live-камерам...",
                            "UML-Модель: ОкноУправленияВызовами", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 3. Кнопка: Направить ближайшую ГБР (include-сценарий из Use Case)
        private void btnAssignGBR_Click(object sender, EventArgs e)
        {
            if (gbrTable == null || gbrTable.Rows.Count == 0)
            {
                gbrTable = new DataTable();
                gbrTable.Columns.Add("ID Экипажа");
                gbrTable.Columns.Add("Название Экипажа");
                gbrTable.Columns.Add("Зона патрулирования");
                gbrTable.Columns.Add("Статус");

                gbrTable.Rows.Add("1", "Экипаж ГБР-ЦЕНТР", "Центральный р-н", "Свободен");
                gbrTable.Rows.Add("2", "Экипаж ГБР-ЗАПАД", "Западный промузел", "Свободен");
                gbrTable.Rows.Add("3", "Экипаж ГБР-ВОСТОК", "Восточный мкр-н", "На вызове");
                dgvGbrTeams.DataSource = gbrTable;
            }
            if (dgvGbrTeams.CurrentRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите кликом мышки свободный экипаж ГБР из нижней таблицы!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Получаем имя выбранного экипажа из таблицы
            string gbrName = dgvGbrTeams.CurrentRow.Cells["Название Экипажа"].Value.ToString();
            string gbrStatus = dgvGbrTeams.CurrentRow.Cells["Статус"].Value.ToString();

            if (gbrStatus == "На вызове")
            {
                MessageBox.Show("Этот экипаж уже занят на другом объекте! Выберите свободную группу.", "Ошибка координации ГБР", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Расчет оптимального маршрута завершен.\nКоординаты объекта №{activeObjectId} переданы на планшет: {gbrName}.",
                            "Интеграция с навигацией ГБР", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Меняем статусы в таблицах на экране
            if (alertTable.Rows.Count > 0)
            {
                alertTable.Rows[alertTable.Rows.Count - 1]["Статус тревоги"] = $"Завершено (Выехал {gbrName})";
            }
            dgvGbrTeams.CurrentRow.Cells["Статус"].Value = "На вызове";

            activeAlertId++;
            activeObjectId += 3; // Меняем ID для следующего теста
            ResetEmergencyBanner();
        }

        // 4. Кнопка: Ложная тревога (Закрытие инцидента диспетчером)
        private void btnFalseAlarm_Click(object sender, EventArgs e)
        {
            if (alertTable.Rows.Count > 0)
            {
                alertTable.Rows[alertTable.Rows.Count - 1]["Статус тревоги"] = "Архив: Ложный вызов";
            }

            MessageBox.Show("Оператор зафиксировал ложный вызов. Задача снята с пульта мониторинга.", "Сброс тревоги");
            activeAlertId++;
            activeObjectId += 2;
            ResetEmergencyBanner();
        }

        private void ResetEmergencyBanner()
        {
            pnlEmergencyBanner.BackColor = Color.LightGray;
            lblAlertStatus.Text = "Система работает в штатном режиме. Сигналов тревоги нет.";
            lblAlertStatus.ForeColor = Color.DimGray;

            btnAcceptAlert.Enabled = false;
            btnAssignGBR.Enabled = false;
            btnFalseAlarm.Enabled = false;
        }

        // Конструктор автоматического создания визуальных элементов пульта ЧОП
        private void InitializeCustomComponents()
        {
            this.Text = "Пульт мониторинга ЧОП — Диспетчерская служба безопасности";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            pnlEmergencyBanner = new Panel { Size = new Size(910, 50), Location = new Point(15, 10), BackColor = Color.LightGray };
            lblAlertStatus = new Label { Text = "Загрузка системы...", Location = new Point(15, 15), Size = new Size(800, 25), Font = new Font("Arial", 11, FontStyle.Bold) };
            pnlEmergencyBanner.Controls.Add(lblAlertStatus);

            btnSimulateAlert = new Button { Text = "ТЕСТ: Сработка датчика", Location = new Point(15, 75), Size = new Size(170, 35), BackColor = Color.MistyRose };
            btnAcceptAlert = new Button { Text = "Принять тревогу", Location = new Point(195, 75), Size = new Size(170, 35), Enabled = false };
            btnAssignGBR = new Button { Text = "Направить ГБР", Location = new Point(375, 75), Size = new Size(170, 35), Enabled = false };
            btnFalseAlarm = new Button { Text = "Ложная тревога", Location = new Point(555, 75), Size = new Size(170, 35), Enabled = false };

            Label lblTable1 = new Label { Text = "Журнал тревог и вызовов (Бизнес-логика Use Case):", Location = new Point(15, 130), Size = new Size(400, 20), Font = new Font("Arial", 9, FontStyle.Underline) };
            dgvAlerts = new DataGridView { Location = new Point(15, 155), Size = new Size(900, 200), AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            Label lblTable2 = new Label { Text = "Мониторинг дежурных экипажей ГБР (Координация на карте):", Location = new Point(15, 375), Size = new Size(500, 20), Font = new Font("Arial", 9, FontStyle.Underline) };
            dgvGbrTeams = new DataGridView { Location = new Point(15, 400), Size = new Size(900, 180), AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            btnSimulateAlert.Click += btnSimulateAlert_Click;
            btnAcceptAlert.Click += btnAcceptAlert_Click;
            btnAssignGBR.Click += btnAssignGBR_Click;
            btnFalseAlarm.Click += btnFalseAlarm_Click;

            this.Controls.AddRange(new Control[] { pnlEmergencyBanner, btnSimulateAlert, btnAcceptAlert, btnAssignGBR, btnFalseAlarm, lblTable1, dgvAlerts, lblTable2, dgvGbrTeams });
        }
    }
}
