using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;

namespace ExpenseTrackerApp
{
    public enum ExpenseCategory
    {
        Oziq_ovqat,
        Transport,
        Uy_haqqi,
        Kiyim,
        Sog_liq,
        O_yin_kulgu,
        Ta_lim,
        Boshqa
    }

    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public ExpenseCategory Category { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }

        public Expense() { }

        public Expense(int id, string description, decimal amount, ExpenseCategory category, DateTime date, string notes = "")
        {
            Id = id;
            Description = description;
            Amount = amount;
            Category = category;
            Date = date;
            Notes = notes;
        }
    }

    public class ExpenseTrackerForm : Form
    {
        private List<Expense> expenses;
        private int nextId = 1;
        private string dataFilePath = "expenses.json";

        private TextBox txtDescription;
        private TextBox txtAmount;
        private ComboBox cmbCategory;
        private DateTimePicker dtpDate;
        private TextBox txtNotes;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnExport;
        private DataGridView dgvExpenses;
        private Label lblTotalExpense;
        private Label lblMonthlyExpense;
        private Label lblCategoryBreakdown;
        private ComboBox cmbFilterCategory;
        private DateTimePicker dtpFilterFrom;
        private DateTimePicker dtpFilterTo;
        private Button btnFilter;
        private Button btnClearFilter;
        private Panel pnlStats;
        private Chart chartExpenses;

        public ExpenseTrackerForm()
        {
            expenses = new List<Expense>();
            InitializeComponents();
            LoadData();
            UpdateStats();
            RefreshGrid();
        }

        private void InitializeComponents()
        {
            this.Text = "Xarajatlar Boshqaruvi";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 250);

            Label lblTitle = new Label
            {
                Text = "💰 Xarajatlar Boshqaruvi",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            this.Controls.Add(lblTitle);

            GroupBox grpInput = new GroupBox
            {
                Text = "Yangi xarajat qo'shish",
                Location = new Point(20, 60),
                Size = new Size(400, 280),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.Controls.Add(grpInput);

            Label lbl1 = new Label
            {
                Text = "Tavsif:",
                Location = new Point(15, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpInput.Controls.Add(lbl1);

            txtDescription = new TextBox
            {
                Location = new Point(15, 55),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 10)
            };
            grpInput.Controls.Add(txtDescription);

            Label lbl2 = new Label
            {
                Text = "Summa (so'm):",
                Location = new Point(15, 85),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpInput.Controls.Add(lbl2);

            txtAmount = new TextBox
            {
                Location = new Point(15, 110),
                Size = new Size(170, 25),
                Font = new Font("Segoe UI", 10)
            };
            grpInput.Controls.Add(txtAmount);

            Label lbl3 = new Label
            {
                Text = "Kategoriya:",
                Location = new Point(205, 85),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpInput.Controls.Add(lbl3);

            cmbCategory = new ComboBox
            {
                Location = new Point(205, 110),
                Size = new Size(170, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbCategory.Items.AddRange(Enum.GetNames(typeof(ExpenseCategory)));
            cmbCategory.SelectedIndex = 0;
            grpInput.Controls.Add(cmbCategory);

            Label lbl4 = new Label
            {
                Text = "Sana:",
                Location = new Point(15, 140),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpInput.Controls.Add(lbl4);

            dtpDate = new DateTimePicker
            {
                Location = new Point(15, 165),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 10)
            };
            grpInput.Controls.Add(dtpDate);

            Label lbl5 = new Label
            {
                Text = "Izoh:",
                Location = new Point(15, 195),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpInput.Controls.Add(lbl5);

            txtNotes = new TextBox
            {
                Location = new Point(15, 220),
                Size = new Size(360, 25),
                Font = new Font("Segoe UI", 10)
            };
            grpInput.Controls.Add(txtNotes);

            GroupBox grpActions = new GroupBox
            {
                Text = "Amallar",
                Location = new Point(20, 350),
                Size = new Size(400, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.Controls.Add(grpActions);

            btnAdd = new Button
            {
                Text = "➕ Qo'shish",
                Location = new Point(15, 30),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;
            grpActions.Controls.Add(btnAdd);

            btnEdit = new Button
            {
                Text = "✏️ Tahrirlash",
                Location = new Point(110, 30),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += BtnEdit_Click;
            grpActions.Controls.Add(btnEdit);

            btnDelete = new Button
            {
                Text = "🗑️ O'chirish",
                Location = new Point(205, 30),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;
            grpActions.Controls.Add(btnDelete);

            btnExport = new Button
            {
                Text = "📊 Export",
                Location = new Point(300, 30),
                Size = new Size(75, 35),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;
            grpActions.Controls.Add(btnExport);

            pnlStats = new Panel
            {
                Location = new Point(20, 440),
                Size = new Size(400, 200),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlStats);

            lblTotalExpense = new Label
            {
                Location = new Point(15, 15),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60)
            };
            pnlStats.Controls.Add(lblTotalExpense);

            lblMonthlyExpense = new Label
            {
                Location = new Point(15, 50),
                Size = new Size(370, 30),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            pnlStats.Controls.Add(lblMonthlyExpense);

            lblCategoryBreakdown = new Label
            {
                Location = new Point(15, 85),
                Size = new Size(370, 100),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(127, 140, 141)
            };
            pnlStats.Controls.Add(lblCategoryBreakdown);

            GroupBox grpFilter = new GroupBox
            {
                Text = "Filter",
                Location = new Point(440, 60),
                Size = new Size(730, 80),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94)
            };
            this.Controls.Add(grpFilter);

            Label lbl6 = new Label
            {
                Text = "Kategoriya:",
                Location = new Point(15, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpFilter.Controls.Add(lbl6);

            cmbFilterCategory = new ComboBox
            {
                Location = new Point(15, 50),
                Size = new Size(120, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbFilterCategory.Items.Add("Barchasi");
            cmbFilterCategory.Items.AddRange(Enum.GetNames(typeof(ExpenseCategory)));
            cmbFilterCategory.SelectedIndex = 0;
            grpFilter.Controls.Add(cmbFilterCategory);

            Label lbl7 = new Label
            {
                Text = "Dan:",
                Location = new Point(150, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpFilter.Controls.Add(lbl7);

            dtpFilterFrom = new DateTimePicker
            {
                Location = new Point(150, 50),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9)
            };
            dtpFilterFrom.Value = DateTime.Now.AddMonths(-1);
            grpFilter.Controls.Add(dtpFilterFrom);

            Label lbl8 = new Label
            {
                Text = "Gacha:",
                Location = new Point(315, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            grpFilter.Controls.Add(lbl8);

            dtpFilterTo = new DateTimePicker
            {
                Location = new Point(315, 50),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9)
            };
            grpFilter.Controls.Add(dtpFilterTo);

            btnFilter = new Button
            {
                Text = "🔍 Qidirish",
                Location = new Point(480, 48),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.Click += BtnFilter_Click;
            grpFilter.Controls.Add(btnFilter);

            btnClearFilter = new Button
            {
                Text = "✖ Tozalash",
                Location = new Point(590, 48),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnClearFilter.FlatAppearance.BorderSize = 0;
            btnClearFilter.Click += BtnClearFilter_Click;
            grpFilter.Controls.Add(btnClearFilter);

            dgvExpenses = new DataGridView
            {
                Location = new Point(440, 150),
                Size = new Size(730, 490),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };
            dgvExpenses.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvExpenses.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvExpenses.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvExpenses.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);
            dgvExpenses.CellDoubleClick += DgvExpenses_CellDoubleClick;
            this.Controls.Add(dgvExpenses);

            dgvExpenses.Columns.Add("Id", "ID");
            dgvExpenses.Columns.Add("Description", "Tavsif");
            dgvExpenses.Columns.Add("Amount", "Summa");
            dgvExpenses.Columns.Add("Category", "Kategoriya");
            dgvExpenses.Columns.Add("Date", "Sana");
            dgvExpenses.Columns.Add("Notes", "Izoh");

            dgvExpenses.Columns[0].Width = 50;
            dgvExpenses.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvExpenses.Columns[4].DefaultCellStyle.Format = "dd.MM.yyyy";
        }

        private void RefreshGrid()
        {
            dgvExpenses.Rows.Clear();
            var displayExpenses = expenses.OrderByDescending(e => e.Date);

            foreach (var exp in displayExpenses)
            {
                dgvExpenses.Rows.Add(
                    exp.Id,
                    exp.Description,
                    exp.Amount.ToString("N0") + " so'm",
                    exp.Category.ToString().Replace("_", " "),
                    exp.Date,
                    exp.Notes
                );
            }
        }

        private void UpdateStats()
        {
            decimal totalExpense = expenses.Sum(e => e.Amount);
            lblTotalExpense.Text = $"Jami xarajat: {totalExpense:N0} so'm";

            var currentMonth = expenses.Where(e => e.Date.Month == DateTime.Now.Month && e.Date.Year == DateTime.Now.Year);
            decimal monthlyExpense = currentMonth.Sum(e => e.Amount);
            lblMonthlyExpense.Text = $"Shu oylik xarajat: {monthlyExpense:N0} so'm";

            var categoryStats = expenses.GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(5);

            string breakdown = "Top 5 kategoriya:\n";
            foreach (var stat in categoryStats)
            {
                breakdown += $"{stat.Category.ToString().Replace("_", " ")}: {stat.Total:N0} so'm\n";
            }
            lblCategoryBreakdown.Text = breakdown;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Tavsif kiriting!", "Xato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("To'g'ri summa kiriting!", "Xato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var expense = new Expense(
                nextId++,
                txtDescription.Text.Trim(),
                amount,
                (ExpenseCategory)Enum.Parse(typeof(ExpenseCategory), cmbCategory.SelectedItem.ToString()),
                dtpDate.Value,
                txtNotes.Text.Trim()
            );

            expenses.Add(expense);
            ClearInputs();
            RefreshGrid();
            UpdateStats();
            SaveData();

            MessageBox.Show("Xarajat muvaffaqiyatli qo'shildi!", "Muvaffaqiyat", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvExpenses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Tahrirlash uchun xarajat tanlang!", "Ogohlantirish", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedId = Convert.ToInt32(dgvExpenses.SelectedRows[0].Cells[0].Value);
            var expense = expenses.FirstOrDefault(e => e.Id == selectedId);

            if (expense != null)
            {
                txtDescription.Text = expense.Description;
                txtAmount.Text = expense.Amount.ToString();
                cmbCategory.SelectedItem = expense.Category.ToString();
                dtpDate.Value = expense.Date;
                txtNotes.Text = expense.Notes;

                expenses.Remove(expense);
                RefreshGrid();
                UpdateStats();
                SaveData();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvExpenses.SelectedRows.Count == 0)
            {
                MessageBox.Show("O'chirish uchun xarajat tanlang!", "Ogohlantirish", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Haqiqatan ham o'chirmoqchimisiz?", "Tasdiqlash", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int selectedId = Convert.ToInt32(dgvExpenses.SelectedRows[0].Cells[0].Value);
                var expense = expenses.FirstOrDefault(e => e.Id == selectedId);

                if (expense != null)
                {
                    expenses.Remove(expense);
                    RefreshGrid();
                    UpdateStats();
                    SaveData();
                    MessageBox.Show("Xarajat o'chirildi!", "Muvaffaqiyat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"Xarajatlar_{DateTime.Now:ddMMyyyy}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.WriteLine("ID,Tavsif,Summa,Kategoriya,Sana,Izoh");
                        foreach (var exp in expenses)
                        {
                            sw.WriteLine($"{exp.Id},{exp.Description},{exp.Amount},{exp.Category},{exp.Date:dd.MM.yyyy},{exp.Notes}");
                        }
                    }
                    MessageBox.Show("Ma'lumotlar eksport qilindi!", "Muvaffaqiyat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Xato: {ex.Message}", "Xato", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            dgvExpenses.Rows.Clear();
            var filtered = expenses.Where(exp => exp.Date >= dtpFilterFrom.Value.Date && exp.Date <= dtpFilterTo.Value.Date);

            if (cmbFilterCategory.SelectedIndex > 0)
            {
                var selectedCategory = (ExpenseCategory)Enum.Parse(typeof(ExpenseCategory), cmbFilterCategory.SelectedItem.ToString());
                filtered = filtered.Where(exp => exp.Category == selectedCategory);
            }

            foreach (var exp in filtered.OrderByDescending(e => e.Date))
            {
                dgvExpenses.Rows.Add(exp.Id, exp.Description, exp.Amount.ToString("N0") + " so'm", exp.Category.ToString().Replace("_", " "), exp.Date, exp.Notes);
            }
        }

        private void BtnClearFilter_Click(object sender, EventArgs e)
        {
            cmbFilterCategory.SelectedIndex = 0;
            dtpFilterFrom.Value = DateTime.Now.AddMonths(-1);
            dtpFilterTo.Value = DateTime.Now;
            RefreshGrid();
        }

        private void DgvExpenses_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                BtnEdit_Click(sender, e);
            }
        }

        private void ClearInputs()
        {
            txtDescription.Clear();
            txtAmount.Clear();
            txtNotes.Clear();
            cmbCategory.SelectedIndex = 0;
            dtpDate.Value = DateTime.Now;
            txtDescription.Focus();
        }

        private void SaveData()
        {
            try
            {
                string json = JsonSerializer.Serialize(expenses, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saqlashda xato: {ex.Message}", "Xato", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    string json = File.ReadAllText(dataFilePath);
                    expenses = JsonSerializer.Deserialize<List<Expense>>(json) ?? new List<Expense>();
                    if (expenses.Any())
                    {
                        nextId = expenses.Max(e => e.Id) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Yuklashda xato: {ex.Message}", "Xato", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ExpenseTrackerForm());
        }
    }

    public class Chart : Panel
    {
    }
}
