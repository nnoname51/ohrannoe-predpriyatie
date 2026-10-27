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
        private Button btnReleaseGBR;
        private DataGridView dgvAlerts;
        private DataGridView dgvGbrTeams;

        // Хранилища данных в оперативной памяти (имитация таблиц БД)
        private DataTable alertTable;
        private DataTable gbrTable;

        public Form1()
        {
            InitializeCustomComponents(); // 1. Сначала создаем красивый интерфейс
            LoadDataTables();             // 2. Затем наполняем его данными
        }

        // Заполнение таблиц демонстрационными данными по вашей UML-модели
        private void LoadDataTables()
        {
            try
            {
                // 1. Инициализация журнала тревог (Таблица [Вызовы])
                alertTable = new DataTable();
                alertTable.Columns.Add("ID Вызова");
                alertTable.Columns.Add("ID Объекта");
                alertTable.Columns.Add("Дата и время");
                alertTable.Columns.Add("Статус тревоги");

                alertTable.Rows.Add("1", "104", DateTime.Now.AddMinutes(-45).ToString("HH:mm:ss"), "Архив: Ложный вызов");
                alertTable.Rows.Add("2", "108", DateTime.Now.AddMinutes(-20).ToString("HH:mm:ss"), "Завершено (Выезд ГБР)");
                dgvAlerts.DataSource = alertTable;

                // 2. Инициализация списка экипажей (Таблица [Экипажи])
                gbrTable = new DataTable();
                gbrTable.Columns.Add("ID Экипажа");
                gbrTable.Columns.Add("Название Экипажа");
                gbrTable.Columns.Add("Зона патрулирования");
                gbrTable.Columns.Add("Статус");

                gbrTable.Rows.Add("1", "Экипаж ГБР-ЦЕНТР", "Центральный р-н", "Свободен");
                gbrTable.Rows.Add("2", "Экипаж ГБР-ЗАПАД", "Западный промузел", "На вызове");
                gbrTable.Rows.Add("3", "Экипаж ГБР-ВОСТОК", "Восточный мкр-н", "Свободен");
                dgvGbrTeams.DataSource = gbrTable;

                ResetEmergencyBanner();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации таблиц: {ex.Message}");
            }
        }

        // 1. Симуляция: Кнопка "Сработка датчика"
        private void btnSimulateAlert_Click(object sender, EventArgs e)
        {
            pnlEmergencyBanner.BackColor = Color.FromArgb(192, 0, 0); // Более мягкий темно-красный
            lblAlertStatus.Text = $"🚨 КРИТИЧЕСКИЙ СИГНАЛ: Сработка датчика на объекте №{activeObjectId}!";
            lblAlertStatus.ForeColor = Color.White;

            btnAcceptAlert.Enabled = true;
            btnAssignGBR.Enabled = false;
            btnFalseAlarm.Enabled = false;

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

            int lastIndex = dgvAlerts.Rows.Count - 1;
            dgvAlerts.CurrentCell = dgvAlerts.Rows[lastIndex].Cells[0];
        }

        // 2. Кнопка: Принять тревогу диспетчером
        private void btnAcceptAlert_Click(object sender, EventArgs e)
        {
            if (dgvAlerts.CurrentRow == null) return;

            pnlEmergencyBanner.BackColor = Color.Orange;
            lblAlertStatus.Text = $"⏳ Обработка тревоги №{activeAlertId}. Изучение карточки объекта...";
            lblAlertStatus.ForeColor = Color.Black;

            btnAcceptAlert.Enabled = false;
            btnAssignGBR.Enabled = true;
            btnFalseAlarm.Enabled = true;

            DataRowView selectedAlert = (DataRowView)dgvAlerts.CurrentRow.DataBoundItem;
            selectedAlert.Row["Статус тревоги"] = "В обработке";

            MessageBox.Show($"Диспетчер изучает данные объекта №{activeObjectId}.\nПроверка договора ЧОП и подключение к live-камерам...",
                            "UML-Модель: ОкноУправленияВызовами", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 3. Кнопка: Направить ближайшую ГБР
        private void btnAssignGBR_Click(object sender, EventArgs e)
        {
            if (dgvGbrTeams.CurrentRow == null || dgvAlerts.CurrentRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите кликом мышки свободный экипаж ГБР из нижней таблицы!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRowView selectedGbrView = (DataRowView)dgvGbrTeams.CurrentRow.DataBoundItem;
            DataRow gbrRow = selectedGbrView.Row;

            string gbrName = gbrRow["Название Экипажа"].ToString();
            string gbrStatus = gbrRow["Статус"].ToString();

            if (gbrStatus == "На вызове")
            {
                MessageBox.Show("Этот экипаж уже занят на другом объекте! Выберите свободную группу.", "Ошибка координации ГБР", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Расчет оптимального маршрута завершен.\nКоординаты объекта №{activeObjectId} переданы на планшет: {gbrName}.",
                            "Интеграция с навигацией ГБР", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DataRowView selectedAlertView = (DataRowView)dgvAlerts.CurrentRow.DataBoundItem;
            selectedAlertView.Row["Статус тревоги"] = $"Завершено (Выехал {gbrName})";

            gbrRow["Статус"] = "На вызове";

            activeAlertId++;
            activeObjectId += 5;
            ResetEmergencyBanner();
        }

        // 4. Кнопка: Ложная тревога
        private void btnFalseAlarm_Click(object sender, EventArgs e)
        {
            if (dgvAlerts.CurrentRow == null) return;

            DataRowView selectedAlertView = (DataRowView)dgvAlerts.CurrentRow.DataBoundItem;
            selectedAlertView.Row["Статус тревоги"] = "Архив: Ложный вызов";

            MessageBox.Show("Оператор зафиксировал ложный вызов. Задача снята с пульта мониторинга.", "Сброс тревоги");
            activeAlertId++;
            activeObjectId += 2;
            ResetEmergencyBanner();
        }

        // 5. Кнопка: Освободить выбранный экипаж
        private void btnReleaseGBR_Click(object sender, EventArgs e)
        {
            if (dgvGbrTeams.CurrentRow == null)
            {
                MessageBox.Show("Выберите экипаж ГБР из нижней таблицы!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataRowView selectedGbrView = (DataRowView)dgvGbrTeams.CurrentRow.DataBoundItem;
            DataRow gbrRow = selectedGbrView.Row;

            string gbrName = gbrRow["Название Экипажа"].ToString();
            string gbrStatus = gbrRow["Статус"].ToString();

            if (gbrStatus == "Свободен")
            {
                MessageBox.Show("Этот экипаж и так находится в режиме патрулирования (Свободен).", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            gbrRow["Статус"] = "Свободен";
            MessageBox.Show($"{gbrName} успешно вернулся на дежурство и снова доступен для выездов!", "Координация ГБР", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetEmergencyBanner()
        {
            pnlEmergencyBanner.BackColor = Color.FromArgb(235, 235, 235);
            lblAlertStatus.Text = "🟢 Система работает в штатном режиме. Сигналов тревоги нет.";
            lblAlertStatus.ForeColor = Color.FromArgb(64, 64, 64);

            btnAcceptAlert.Enabled = false;
            btnAssignGBR.Enabled = false;
            btnFalseAlarm.Enabled = false;
        }

        // Конструктор автоматического создания визуальных элементов пульта ЧОП
        private void InitializeCustomComponents()
        {
            this.Text = "Пульт мониторинга ЧОП — Диспетчерская служба безопасности";
            this.Size = new Size(950, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White; // Белый чистый фон формы

            // 1. Верхний информационный баннер
            pnlEmergencyBanner = new Panel { Size = new Size(910, 55), Location = new Point(15, 10), BorderStyle = BorderStyle.None };
            lblAlertStatus = new Label { Text = "Загрузка системы...", Location = new Point(15, 16), Size = new Size(880, 25), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            pnlEmergencyBanner.Controls.Add(lblAlertStatus);

            // Стилизация Flat-кнопок
            btnSimulateAlert = new Button { Text = "Сработка датчика", Location = new Point(15, 80), Size = new Size(170, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 210, 210), ForeColor = Color.FromArgb(150, 0, 0), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnSimulateAlert.FlatAppearance.BorderColor = Color.FromArgb(240, 150, 150);

            btnAcceptAlert = new Button { Text = "Принять тревогу", Location = new Point(195, 80), Size = new Size(170, 38), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Regular), Enabled = false };
            btnAssignGBR = new Button { Text = "Направить ГБР", Location = new Point(375, 80), Size = new Size(170, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(220, 245, 220), ForeColor = Color.FromArgb(0, 100, 0), Font = new Font("Segoe UI", 9, FontStyle.Bold), Enabled = false };
            btnAssignGBR.FlatAppearance.BorderColor = Color.FromArgb(170, 220, 170);
            btnFalseAlarm = new Button { Text = "Ложная тревога", Location = new Point(555, 80), Size = new Size(170, 38), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Regular), Enabled = false };
            btnReleaseGBR = new Button { Text = "Освободить экипаж", Location = new Point(735, 80), Size = new Size(170, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(225, 240, 255), ForeColor = Color.FromArgb(0, 90, 180), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnReleaseGBR.FlatAppearance.BorderColor = Color.FromArgb(170, 200, 240);
            // Текстовые заголовки
            Label lblTable1 = new Label { Text = "Журнал тревог и вызовов (Бизнес-логика Use Case)", Location = new Point(15, 140), Size = new Size(400, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 70, 70) };
            Label lblTable2 = new Label { Text = "Мониторинг дежурных экипажей ГБР (Координация на карте)", Location = new Point(15, 385), Size = new Size(500, 20), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(70, 70, 70) };
            // 2. Настройка верхней таблицы вызовов
            dgvAlerts = new DataGridView { Location = new Point(15, 165), Size = new Size(900, 200), AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.FromArgb(245, 245, 245), BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            dgvAlerts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAlerts.MultiSelect = false;
            dgvAlerts.GridColor = Color.Gainsboro;
            dgvAlerts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            // 3. Настройка нижней таблицы экипажей ГБР
            dgvGbrTeams = new DataGridView { Location = new Point(15, 410), Size = new Size(900, 250), AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.FromArgb(245, 245, 245), BorderStyle = BorderStyle.None, RowHeadersVisible = false };
            dgvGbrTeams.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGbrTeams.MultiSelect = false;
            dgvGbrTeams.GridColor = Color.Gainsboro;
            dgvGbrTeams.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);
            // Привязка событий клика
            btnSimulateAlert.Click += btnSimulateAlert_Click;
            btnAcceptAlert.Click += btnAcceptAlert_Click;
            btnAssignGBR.Click += btnAssignGBR_Click;
            btnFalseAlarm.Click += btnFalseAlarm_Click;
            btnReleaseGBR.Click += btnReleaseGBR_Click;
            // Добавление элементов на экран
            this.Controls.AddRange(new Control[] { pnlEmergencyBanner, btnSimulateAlert, btnAcceptAlert, btnAssignGBR, btnFalseAlarm, btnReleaseGBR, lblTable1, dgvAlerts, lblTable2, dgvGbrTeams });
        }
    }
}

