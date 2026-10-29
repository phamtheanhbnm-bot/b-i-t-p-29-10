using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CafePOSSystem
{
    public partial class Form1 : Form
    {
        private readonly string connectionString =
            @"Data Source=(local);Initial Catalog=CafePOSDB;Integrated Security=True;TrustServerCertificate=True;";

        private TabControl tabs;
        private FlowLayoutPanel tablePanel;
        private DataGridView dgvOrder;
        private DataGridView dgvMenu;
        private ComboBox cboCategory;
        private ComboBox cboProduct;
        private ComboBox cboMenuCategory;
        private NumericUpDown numQuantity;
        private Label lblSelectedTable;
        private Label lblTotal;
        private TextBox txtName;
        private TextBox txtPrice;

        private int selectedTableId = -1;
        private int selectedMenuId = -1;

        public Form1()
        {
            InitializeComponent();

            Text = "CAFE POS SYSTEM";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 750);
            MinimumSize = new Size(900, 600);
            Font = new Font("Segoe UI", 10);

            BuildInterface();

            Shown += (s, e) =>
            {
                try
                {
                    LoadCategories();
                    LoadTables();
                    LoadProducts();
                    LoadMenuGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Lỗi kết nối hoặc tải dữ liệu:\n" + ex.Message);
                }
            };
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        private void BuildInterface()
        {
            tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };

            TabPage orderPage = new TabPage("Bán hàng");
            TabPage menuPage = new TabPage("Quản lý thực đơn");

            tabs.TabPages.Add(orderPage);
            tabs.TabPages.Add(menuPage);
            Controls.Add(tabs);

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Size = new Size(1100, 650),
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                Panel1MinSize = 150,
                Panel2MinSize = 350,
                SplitterDistance = 360
            };

            orderPage.Controls.Add(split);

            Label lblTables = new Label
            {
                Text = "SƠ ĐỒ BÀN",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 15, FontStyle.Bold)
            };

            tablePanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(5)
            };

            split.Panel1.Controls.Add(lblTables);
            split.Panel1.Controls.Add(tablePanel);
            tablePanel.BringToFront();
            lblTables.BringToFront();

            Panel rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            split.Panel2.Controls.Add(rightPanel);

            Label lblOrderTitle = new Label
            {
                Text = "GỌI MÓN",
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 15, FontStyle.Bold)
            };

            rightPanel.Controls.Add(lblOrderTitle);

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 85
            };

            rightPanel.Controls.Add(bottomPanel);

            lblTotal = new Label
            {
                Text = "Tổng tiền: 0 đ",
                Dock = DockStyle.Top,
                Height = 35,
                Font = new Font("Segoe UI", 13, FontStyle.Bold)
            };

            bottomPanel.Controls.Add(lblTotal);

            Button btnRemove = new Button
            {
                Text = "Xóa dòng",
                Location = new Point(0, 40),
                Size = new Size(110, 35)
            };

            btnRemove.Click += (s, e) => RemoveOrderItem();
            bottomPanel.Controls.Add(btnRemove);

            Button btnPay = new Button
            {
                Text = "THANH TOÁN",
                Location = new Point(120, 40),
                Size = new Size(150, 35),
                BackColor = Color.LightGreen
            };

            btnPay.Click += (s, e) => PayBill();
            bottomPanel.Controls.Add(btnPay);

            lblSelectedTable = new Label
            {
                Text = "Chưa chọn bàn",
                Dock = DockStyle.Top,
                Height = 30
            };

            rightPanel.Controls.Add(lblSelectedTable);

            Panel inputPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 145
            };

            rightPanel.Controls.Add(inputPanel);

            AddLabel(inputPanel, "Danh mục:", 0, 5);

            cboCategory = new ComboBox
            {
                Location = new Point(100, 2),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cboCategory.SelectedIndexChanged += (s, e) =>
            {
                if (IsHandleCreated)
                    LoadProducts();
            };

            inputPanel.Controls.Add(cboCategory);

            AddLabel(inputPanel, "Món ăn:", 0, 42);

            cboProduct = new ComboBox
            {
                Location = new Point(100, 39),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            inputPanel.Controls.Add(cboProduct);

            AddLabel(inputPanel, "Số lượng:", 0, 79);

            numQuantity = new NumericUpDown
            {
                Location = new Point(100, 76),
                Minimum = 1,
                Maximum = 999,
                Value = 1
            };

            inputPanel.Controls.Add(numQuantity);

            Button btnAddOrder = new Button
            {
                Text = "Thêm món",
                Location = new Point(0, 110),
                Size = new Size(120, 32)
            };

            btnAddOrder.Click += (s, e) => AddOrder();
            inputPanel.Controls.Add(btnAddOrder);

            dgvOrder = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            rightPanel.Controls.Add(dgvOrder);

            BuildMenuPage(menuPage);
        }

        private void AddLabel(
            Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private void BuildMenuPage(TabPage page)
        {
            Panel top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150
            };

            Label title = new Label
            {
                Text = "QUẢN LÝ THỰC ĐƠN",
                Location = new Point(10, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 15, FontStyle.Bold)
            };

            top.Controls.Add(title);

            AddLabel(top, "Tên món:", 10, 45);

            txtName = new TextBox
            {
                Location = new Point(100, 42),
                Width = 220
            };

            top.Controls.Add(txtName);

            AddLabel(top, "Đơn giá:", 350, 45);

            txtPrice = new TextBox
            {
                Location = new Point(420, 42),
                Width = 160
            };

            top.Controls.Add(txtPrice);

            AddLabel(top, "Danh mục:", 10, 82);

            cboMenuCategory = new ComboBox
            {
                Location = new Point(100, 79),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            top.Controls.Add(cboMenuCategory);

            Button btnAdd = new Button
            {
                Text = "Thêm món",
                Location = new Point(350, 78),
                Size = new Size(100, 32)
            };

            btnAdd.Click += (s, e) => SaveMenu(false);
            top.Controls.Add(btnAdd);

            Button btnEdit = new Button
            {
                Text = "Sửa món",
                Location = new Point(460, 78),
                Size = new Size(100, 32)
            };

            btnEdit.Click += (s, e) => SaveMenu(true);
            top.Controls.Add(btnEdit);

            Button btnDelete = new Button
            {
                Text = "Xóa món",
                Location = new Point(570, 78),
                Size = new Size(100, 32)
            };

            btnDelete.Click += (s, e) => DeleteMenu();
            top.Controls.Add(btnDelete);

            Button btnClear = new Button
            {
                Text = "Làm mới",
                Location = new Point(680, 78),
                Size = new Size(100, 32)
            };

            btnClear.Click += (s, e) => ClearMenuInputs();
            top.Controls.Add(btnClear);

            dgvMenu = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            page.Controls.Add(dgvMenu);
            page.Controls.Add(top);

            dgvMenu.SelectionChanged += (s, e) =>
            {
                if (dgvMenu.CurrentRow == null ||
                    dgvMenu.CurrentRow.IsNewRow)
                    return;

                try
                {
                    selectedMenuId = Convert.ToInt32(
                        dgvMenu.CurrentRow.Cells["MaMon"].Value);

                    txtName.Text = Convert.ToString(
                        dgvMenu.CurrentRow.Cells["TenMon"].Value);

                    txtPrice.Text = Convert.ToDecimal(
                        dgvMenu.CurrentRow.Cells["DonGia"].Value)
                        .ToString();

                    if (cboMenuCategory.DataSource != null)
                    {
                        cboMenuCategory.SelectedValue =
                            Convert.ToInt32(
                                dgvMenu.CurrentRow.Cells[
                                    "MaDanhMuc"].Value);
                    }
                }
                catch
                {
                }
            };
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                DataTable dt = new DataTable();

                using (SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT MaDanhMuc, TenDanhMuc
                      FROM DanhMuc ORDER BY MaDanhMuc", conn))
                {
                    da.Fill(dt);
                }

                cboCategory.DataSource = dt.Copy();
                cboCategory.DisplayMember = "TenDanhMuc";
                cboCategory.ValueMember = "MaDanhMuc";

                cboMenuCategory.DataSource = dt;
                cboMenuCategory.DisplayMember = "TenDanhMuc";
                cboMenuCategory.ValueMember = "MaDanhMuc";
            }
        }

        private void LoadProducts()
        {
            if (cboCategory == null ||
                cboProduct == null ||
                cboCategory.SelectedValue == null ||
                cboCategory.SelectedValue is DataRowView)
                return;

            int categoryId =
                Convert.ToInt32(cboCategory.SelectedValue);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                DataTable dt = new DataTable();

                using (SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT MaMon, TenMon, DonGia
                      FROM MonAn
                      WHERE MaDanhMuc = @id
                      ORDER BY TenMon", conn))
                {
                    da.SelectCommand.Parameters.Add(
                        "@id", SqlDbType.Int).Value = categoryId;

                    da.Fill(dt);
                }

                cboProduct.DataSource = dt;
                cboProduct.DisplayMember = "TenMon";
                cboProduct.ValueMember = "MaMon";
            }
        }

        private void LoadTables()
        {
            tablePanel.Controls.Clear();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                DataTable dt = new DataTable();

                using (SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT MaBan, TenBan, TrangThai
                      FROM BanAn ORDER BY MaBan", conn))
                {
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["MaBan"]);
                    string name = Convert.ToString(row["TenBan"]);
                    string status =
                        Convert.ToString(row["TrangThai"]);

                    Button btn = new Button
                    {
                        Text = name + "\n" + status,
                        Tag = id,
                        Size = new Size(125, 75),
                        Margin = new Padding(8),
                        BackColor = status == "Trống"
                            ? Color.LightGreen
                            : Color.LightCoral
                    };

                    btn.Click += (s, e) =>
                    {
                        selectedTableId = id;
                        lblSelectedTable.Text =
                            "Bàn đang chọn: " + name;
                        LoadOrder();
                    };

                    tablePanel.Controls.Add(btn);
                }
            }
        }

        private int GetOpenBill(
            SqlConnection conn,
            int tableId,
            SqlTransaction tran = null)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 1 MaHoaDon
                  FROM HoaDon
                  WHERE MaBan = @ban AND TrangThai = 0
                  ORDER BY MaHoaDon DESC", conn, tran))
            {
                cmd.Parameters.Add(
                    "@ban", SqlDbType.Int).Value = tableId;

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return 0;

                return Convert.ToInt32(result);
            }
        }

        private void LoadOrder()
        {
            if (selectedTableId < 0)
                return;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                int billId = GetOpenBill(
                    conn, selectedTableId);

                DataTable dt = new DataTable();

                if (billId > 0)
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(
                        @"SELECT ct.MaMon, m.TenMon,
                                 ct.DonGia, ct.SoLuong,
                                 ct.DonGia * ct.SoLuong AS ThanhTien
                          FROM ChiTietHoaDon ct
                          INNER JOIN MonAn m
                          ON ct.MaMon = m.MaMon
                          WHERE ct.MaHoaDon = @id
                          ORDER BY m.TenMon", conn))
                    {
                        da.SelectCommand.Parameters.Add(
                            "@id", SqlDbType.Int).Value = billId;

                        da.Fill(dt);
                    }
                }
                else
                {
                    dt.Columns.Add("MaMon", typeof(int));
                    dt.Columns.Add("TenMon", typeof(string));
                    dt.Columns.Add("DonGia", typeof(decimal));
                    dt.Columns.Add("SoLuong", typeof(int));
                    dt.Columns.Add("ThanhTien", typeof(decimal));
                }

                dgvOrder.DataSource = dt;

                decimal total = 0;

                foreach (DataRow row in dt.Rows)
                {
                    total += Convert.ToDecimal(
                        row["ThanhTien"]);
                }

                lblTotal.Text =
                    "Tổng tiền: " + total.ToString("N0") + " đ";
            }
        }

        private void AddOrder()
        {
            if (selectedTableId < 0)
            {
                MessageBox.Show("Hãy chọn bàn trước.");
                return;
            }

            if (cboProduct.SelectedValue == null ||
                cboProduct.SelectedValue is DataRowView)
            {
                MessageBox.Show("Danh mục chưa có món ăn.");
                return;
            }

            int productId =
                Convert.ToInt32(cboProduct.SelectedValue);

            int quantity = (int)numQuantity.Value;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int billId = GetOpenBill(
                            conn, selectedTableId, tran);

                        if (billId == 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO HoaDon
                                  (MaBan, GioVao, TongTien, TrangThai)
                                  VALUES (@ban, GETDATE(), 0, 0);
                                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                                conn, tran))
                            {
                                cmd.Parameters.Add(
                                    "@ban", SqlDbType.Int).Value =
                                    selectedTableId;

                                billId = Convert.ToInt32(
                                    cmd.ExecuteScalar());
                            }
                        }

                        decimal price;

                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT DonGia FROM MonAn WHERE MaMon=@mon",
                            conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@mon", SqlDbType.Int).Value =
                                productId;

                            object result = cmd.ExecuteScalar();

                            if (result == null ||
                                result == DBNull.Value)
                                throw new Exception(
                                    "Không tìm thấy món ăn.");

                            price = Convert.ToDecimal(result);
                        }

                        int affected;

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE ChiTietHoaDon
                              SET SoLuong = SoLuong + @sl
                              WHERE MaHoaDon = @hd
                              AND MaMon = @mon", conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@sl", SqlDbType.Int).Value = quantity;

                            cmd.Parameters.Add(
                                "@hd", SqlDbType.Int).Value = billId;

                            cmd.Parameters.Add(
                                "@mon", SqlDbType.Int).Value =
                                productId;

                            affected = cmd.ExecuteNonQuery();
                        }

                        if (affected == 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO ChiTietHoaDon
                                  (MaHoaDon, MaMon, SoLuong, DonGia)
                                  VALUES (@hd, @mon, @sl, @gia)",
                                conn, tran))
                            {
                                cmd.Parameters.Add(
                                    "@hd", SqlDbType.Int).Value = billId;

                                cmd.Parameters.Add(
                                    "@mon", SqlDbType.Int).Value =
                                    productId;

                                cmd.Parameters.Add(
                                    "@sl", SqlDbType.Int).Value =
                                    quantity;

                                cmd.Parameters.Add(
                                    "@gia", SqlDbType.Decimal).Value =
                                    price;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE BanAn
                              SET TrangThai = N'Có khách'
                              WHERE MaBan = @ban", conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@ban", SqlDbType.Int).Value =
                                selectedTableId;

                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tran.Rollback(); } catch { }

                        MessageBox.Show(
                            "Lỗi thêm món: " + ex.Message);
                        return;
                    }
                }
            }

            LoadTables();
            LoadOrder();
        }

        private void RemoveOrderItem()
        {
            if (selectedTableId < 0 ||
                dgvOrder.CurrentRow == null ||
                dgvOrder.CurrentRow.IsNewRow)
            {
                MessageBox.Show(
                    "Hãy chọn bàn và dòng món cần xóa.");
                return;
            }

            int productId = Convert.ToInt32(
                dgvOrder.CurrentRow.Cells["MaMon"].Value);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                int billId = GetOpenBill(
                    conn, selectedTableId);

                if (billId == 0)
                    return;

                using (SqlCommand cmd = new SqlCommand(
                    @"DELETE FROM ChiTietHoaDon
                      WHERE MaHoaDon = @hd AND MaMon = @mon",
                    conn))
                {
                    cmd.Parameters.Add(
                        "@hd", SqlDbType.Int).Value = billId;

                    cmd.Parameters.Add(
                        "@mon", SqlDbType.Int).Value = productId;

                    cmd.ExecuteNonQuery();
                }
            }

            LoadOrder();
        }

        private void PayBill()
        {
            if (selectedTableId < 0)
            {
                MessageBox.Show("Hãy chọn bàn cần thanh toán.");
                return;
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        int billId = GetOpenBill(
                            conn, selectedTableId, tran);

                        if (billId == 0)
                        {
                            tran.Rollback();
                            MessageBox.Show("Bàn chưa có hóa đơn.");
                            return;
                        }

                        decimal total;

                        using (SqlCommand cmd = new SqlCommand(
                            @"SELECT ISNULL(SUM(SoLuong * DonGia), 0)
                              FROM ChiTietHoaDon
                              WHERE MaHoaDon = @hd",
                            conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@hd", SqlDbType.Int).Value = billId;

                            total = Convert.ToDecimal(
                                cmd.ExecuteScalar());
                        }

                        if (total <= 0)
                        {
                            tran.Rollback();
                            MessageBox.Show("Hóa đơn chưa có món.");
                            return;
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE HoaDon
                              SET GioRa = GETDATE(),
                                  TongTien = @tien,
                                  TrangThai = 1
                              WHERE MaHoaDon = @hd
                              AND TrangThai = 0", conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@tien", SqlDbType.Decimal).Value = total;

                            cmd.Parameters.Add(
                                "@hd", SqlDbType.Int).Value = billId;

                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE BanAn
                              SET TrangThai = N'Trống'
                              WHERE MaBan = @ban", conn, tran))
                        {
                            cmd.Parameters.Add(
                                "@ban", SqlDbType.Int).Value =
                                selectedTableId;

                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();

                        MessageBox.Show(
                            "Thanh toán thành công!\nTổng tiền: " +
                            total.ToString("N0") + " đ");
                    }
                    catch (Exception ex)
                    {
                        try { tran.Rollback(); } catch { }

                        MessageBox.Show(
                            "Lỗi thanh toán: " + ex.Message);
                        return;
                    }
                }
            }

            LoadTables();
            LoadOrder();
        }

        private void SaveMenu(bool edit)
        {
            string name = txtName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Tên món không được để trống.");
                txtName.Focus();
                return;
            }

            if (!decimal.TryParse(
                txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show(
                    "Đơn giá phải là số không âm.");
                txtPrice.Focus();
                return;
            }

            if (cboMenuCategory.SelectedValue == null ||
                cboMenuCategory.SelectedValue is DataRowView)
            {
                MessageBox.Show("Hãy chọn danh mục.");
                return;
            }

            if (edit && selectedMenuId < 0)
            {
                MessageBox.Show("Hãy chọn món cần sửa.");
                return;
            }

            int categoryId = Convert.ToInt32(
                cboMenuCategory.SelectedValue);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                string sql = edit
                    ? @"UPDATE MonAn
                        SET TenMon = @ten,
                            MaDanhMuc = @dm,
                            DonGia = @gia
                        WHERE MaMon = @id"
                    : @"INSERT INTO MonAn
                        (TenMon, MaDanhMuc, DonGia)
                        VALUES (@ten, @dm, @gia)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(
                        "@ten", SqlDbType.NVarChar, 100).Value = name;

                    cmd.Parameters.Add(
                        "@dm", SqlDbType.Int).Value = categoryId;

                    SqlParameter p = cmd.Parameters.Add(
                        "@gia", SqlDbType.Decimal);

                    p.Precision = 18;
                    p.Scale = 2;
                    p.Value = price;

                    if (edit)
                    {
                        cmd.Parameters.Add(
                            "@id", SqlDbType.Int).Value =
                            selectedMenuId;
                    }

                    cmd.ExecuteNonQuery();
                }
            }

            LoadMenuGrid();
            LoadProducts();
            ClearMenuInputs();

            MessageBox.Show(
                edit ? "Đã sửa món." : "Đã thêm món.");
        }

        private void DeleteMenu()
        {
            if (selectedMenuId < 0)
            {
                MessageBox.Show("Hãy chọn món cần xóa.");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Bạn có chắc muốn xóa món này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM MonAn WHERE MaMon = @id",
                        conn))
                    {
                        cmd.Parameters.Add(
                            "@id", SqlDbType.Int).Value =
                            selectedMenuId;

                        cmd.ExecuteNonQuery();
                    }
                }

                LoadMenuGrid();
                LoadProducts();
                ClearMenuInputs();

                MessageBox.Show("Đã xóa món.");
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Không thể xóa món đã được sử dụng trong hóa đơn.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa món: " + ex.Message);
            }
        }

        private void LoadMenuGrid()
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                DataTable dt = new DataTable();

                using (SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT MaMon, TenMon, MaDanhMuc, DonGia
                      FROM MonAn ORDER BY MaMon DESC", conn))
                {
                    da.Fill(dt);
                }

                dgvMenu.DataSource = dt;
            }
        }

        private void ClearMenuInputs()
        {
            selectedMenuId = -1;
            txtName.Clear();
            txtPrice.Clear();

            if (dgvMenu != null)
                dgvMenu.ClearSelection();
        }
    }
}