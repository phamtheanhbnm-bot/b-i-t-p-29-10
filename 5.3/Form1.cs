
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CafePOSSystem
{
    public class Form1 : Form
    {
        private readonly string connectionString =
            @"Data Source=(local);Initial Catalog=CafePOSDB;Integrated Security=True;TrustServerCertificate=True;";

        private TabControl tabs;
        private FlowLayoutPanel tablePanel;
        private DataGridView dgvOrder;
        private DataGridView dgvMenu;
        private ComboBox cboCategory;
        private ComboBox cboProduct;
        private NumericUpDown numQuantity;
        private Label lblSelectedTable;
        private Label lblTotal;
        private TextBox txtName;
        private TextBox txtPrice;
        private ComboBox cboMenuCategory;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;

        private int selectedTableId = -1;
        private string selectedTableName = "";
        private int selectedMenuId = -1;

        public Form1()
        {
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
                    LoadTables();
                    LoadCategories();
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không tải được dữ liệu:\n" + ex.Message,
                        "Lỗi kết nối SQL Server",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };
        }

        private void BuildInterface()
        {
            tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;

            TabPage orderPage = new TabPage("Bán hàng");
            TabPage menuPage = new TabPage("Quản lý thực đơn");

            tabs.TabPages.Add(orderPage);
            tabs.TabPages.Add(menuPage);
            Controls.Add(tabs);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Vertical;
            split.Panel1MinSize = 0;
            split.Panel2MinSize = 0;

            orderPage.Controls.Add(split);

            Panel leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Padding = new Padding(10);
            leftPanel.AutoScroll = true;
            split.Panel1.Controls.Add(leftPanel);

            Label titleTables = new Label();
            titleTables.Text = "SƠ ĐỒ BÀN";
            titleTables.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            titleTables.Dock = DockStyle.Top;
            titleTables.Height = 45;
            leftPanel.Controls.Add(titleTables);

            tablePanel = new FlowLayoutPanel();
            tablePanel.Dock = DockStyle.Fill;
            tablePanel.AutoScroll = true;
            tablePanel.Padding = new Padding(5);
            leftPanel.Controls.Add(tablePanel);
            tablePanel.BringToFront();

            Panel rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Padding = new Padding(10);
            split.Panel2.Controls.Add(rightPanel);

            Label titleOrder = new Label();
            titleOrder.Text = "GỌI MÓN";
            titleOrder.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            titleOrder.Dock = DockStyle.Top;
            titleOrder.Height = 40;
            rightPanel.Controls.Add(titleOrder);

            Panel orderContent = new Panel();
            orderContent.Dock = DockStyle.Fill;
            rightPanel.Controls.Add(orderContent);
            orderContent.BringToFront();

            lblSelectedTable = new Label();
            lblSelectedTable.Text = "Chưa chọn bàn";
            lblSelectedTable.Dock = DockStyle.Top;
            lblSelectedTable.Height = 32;
            orderContent.Controls.Add(lblSelectedTable);

            Panel inputPanel = new Panel();
            inputPanel.Dock = DockStyle.Top;
            inputPanel.Height = 150;
            orderContent.Controls.Add(inputPanel);

            Label lblCategory = new Label();
            lblCategory.Text = "Danh mục:";
            lblCategory.Location = new Point(0, 5);
            lblCategory.AutoSize = true;
            inputPanel.Controls.Add(lblCategory);

            cboCategory = new ComboBox();
            cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategory.Location = new Point(100, 2);
            cboCategory.Width = 220;
            inputPanel.Controls.Add(cboCategory);
            cboCategory.SelectedIndexChanged += (s, e) =>
            {
                LoadProducts();
            };

            Label lblProduct = new Label();
            lblProduct.Text = "Món ăn:";
            lblProduct.Location = new Point(0, 42);
            lblProduct.AutoSize = true;
            inputPanel.Controls.Add(lblProduct);

            cboProduct = new ComboBox();
            cboProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            cboProduct.Location = new Point(100, 39);
            cboProduct.Width = 220;
            inputPanel.Controls.Add(cboProduct);

            Label lblQuantity = new Label();
            lblQuantity.Text = "Số lượng:";
            lblQuantity.Location = new Point(0, 79);
            lblQuantity.AutoSize = true;
            inputPanel.Controls.Add(lblQuantity);

            numQuantity = new NumericUpDown();
            numQuantity.Location = new Point(100, 76);
            numQuantity.Minimum = 1;
            numQuantity.Maximum = 999;
            numQuantity.Value = 1;
            inputPanel.Controls.Add(numQuantity);

            Button btnAddOrder = new Button();
            btnAddOrder.Text = "Thêm món";
            btnAddOrder.Location = new Point(0, 112);
            btnAddOrder.Size = new Size(120, 32);
            btnAddOrder.Click += (s, e) => AddOrder();
            inputPanel.Controls.Add(btnAddOrder);

            dgvOrder = new DataGridView();
            dgvOrder.Dock = DockStyle.Fill;
            dgvOrder.ReadOnly = true;
            dgvOrder.AllowUserToAddRows = false;
            dgvOrder.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrder.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            orderContent.Controls.Add(dgvOrder);
            dgvOrder.BringToFront();

            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 85;
            orderContent.Controls.Add(bottomPanel);
            bottomPanel.BringToFront();

            lblTotal = new Label();
            lblTotal.Text = "Tổng tiền: 0 đ";
            lblTotal.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTotal.Dock = DockStyle.Top;
            lblTotal.Height = 35;
            bottomPanel.Controls.Add(lblTotal);

            Button btnRemoveOrder = new Button();
            btnRemoveOrder.Text = "Xóa dòng";
            btnRemoveOrder.Location = new Point(0, 42);
            btnRemoveOrder.Size = new Size(110, 35);
            btnRemoveOrder.Click += (s, e) => RemoveOrderItem();
            bottomPanel.Controls.Add(btnRemoveOrder);

            Button btnPay = new Button();
            btnPay.Text = "THANH TOÁN";
            btnPay.BackColor = Color.LightGreen;
            btnPay.Location = new Point(120, 42);
            btnPay.Size = new Size(150, 35);
            btnPay.Click += (s, e) => PayBill();
            bottomPanel.Controls.Add(btnPay);

            BuildMenuPage(menuPage);
        }

        private void BuildMenuPage(TabPage page)
        {
            Panel top = new Panel();
            top.Dock = DockStyle.Top;
            top.Height = 160;
            top.Padding = new Padding(10);
            page.Controls.Add(top);

            Label lblTitle = new Label();
            lblTitle.Text = "QUẢN LÝ THỰC ĐƠN";
            lblTitle.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            lblTitle.Location = new Point(10, 5);
            lblTitle.AutoSize = true;
            top.Controls.Add(lblTitle);

            Label lblName = new Label();
            lblName.Text = "Tên món:";
            lblName.Location = new Point(10, 45);
            lblName.AutoSize = true;
            top.Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Location = new Point(100, 42);
            txtName.Width = 220;
            top.Controls.Add(txtName);

            Label lblPrice = new Label();
            lblPrice.Text = "Đơn giá:";
            lblPrice.Location = new Point(350, 45);
            lblPrice.AutoSize = true;
            top.Controls.Add(lblPrice);

            txtPrice = new TextBox();
            txtPrice.Location = new Point(420, 42);
            txtPrice.Width = 160;
            top.Controls.Add(txtPrice);

            Label lblCat = new Label();
            lblCat.Text = "Danh mục:";
            lblCat.Location = new Point(10, 82);
            lblCat.AutoSize = true;
            top.Controls.Add(lblCat);

            cboMenuCategory = new ComboBox();
            cboMenuCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMenuCategory.Location = new Point(100, 79);
            cboMenuCategory.Width = 220;
            top.Controls.Add(cboMenuCategory);

            btnAdd = new Button();
            btnAdd.Text = "Thêm món";
            btnAdd.Location = new Point(350, 78);
            btnAdd.Size = new Size(100, 32);
            btnAdd.Click += (s, e) => SaveMenu(false);
            top.Controls.Add(btnAdd);

            btnEdit = new Button();
            btnEdit.Text = "Sửa món";
            btnEdit.Location = new Point(460, 78);
            btnEdit.Size = new Size(100, 32);
            btnEdit.Click += (s, e) => SaveMenu(true);
            top.Controls.Add(btnEdit);

            btnDelete = new Button();
            btnDelete.Text = "Xóa món";
            btnDelete.Location = new Point(570, 78);
            btnDelete.Size = new Size(100, 32);
            btnDelete.Click += (s, e) => DeleteMenu();
            top.Controls.Add(btnDelete);

            Button btnClear = new Button();
            btnClear.Text = "Làm mới";
            btnClear.Location = new Point(680, 78);
            btnClear.Size = new Size(100, 32);
            btnClear.Click += (s, e) => ClearMenuInputs();
            top.Controls.Add(btnClear);

            dgvMenu = new DataGridView();
            dgvMenu.Dock = DockStyle.Fill;
            dgvMenu.ReadOnly = true;
            dgvMenu.AllowUserToAddRows = false;
            dgvMenu.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvMenu.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            page.Controls.Add(dgvMenu);
            dgvMenu.BringToFront();

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

                    txtPrice.Text = Convert.ToString(
                        dgvMenu.CurrentRow.Cells["DonGia"].Value);

                    if (cboMenuCategory.DataSource != null)
                    {
                        cboMenuCategory.SelectedValue =
                            Convert.ToInt32(
                                dgvMenu.CurrentRow.Cells["MaDanhMuc"].Value);
                    }
                }
                catch
                {
                }
            };
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        private void LoadTables()
        {
            tablePanel.Controls.Clear();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT MaBan, TenBan, TrangThai FROM BanAn ORDER BY MaBan",
                    conn))
                {
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["MaBan"]);
                    string name = row["TenBan"].ToString();
                    string status = row["TrangThai"].ToString();

                    Button btn = new Button();
                    btn.Text = name + "\n" + status;
                    btn.Tag = id;
                    btn.Size = new Size(125, 75);
                    btn.Margin = new Padding(8);
                    btn.BackColor = status == "Trống"
                        ? Color.LightGreen
                        : Color.LightCoral;

                    btn.Click += (s, e) =>
                    {
                        selectedTableId = id;
                        selectedTableName = name;
                        lblSelectedTable.Text =
                            "Bàn đang chọn: " + name;

                        LoadOrder();
                    };

                    tablePanel.Controls.Add(btn);
                }
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc ORDER BY TenDanhMuc",
                    conn))
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
            if (cboProduct == null || cboCategory == null)
                return;

            if (cboCategory.SelectedValue == null ||
                cboCategory.SelectedValue is DataRowView)
                return;

            int categoryId = Convert.ToInt32(
                cboCategory.SelectedValue);

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
                    da.SelectCommand.Parameters.AddWithValue(
                        "@id", categoryId);
                    da.Fill(dt);
                }

                cboProduct.DataSource = dt;
                cboProduct.DisplayMember = "TenMon";
                cboProduct.ValueMember = "MaMon";
            }
        }

        private int GetOpenBill(SqlConnection conn, int tableId,
            SqlTransaction transaction = null)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 1 MaHoaDon
                  FROM HoaDon
                  WHERE MaBan = @ban AND TrangThai = 0
                  ORDER BY MaHoaDon DESC",
                conn, transaction))
            {
                cmd.Parameters.AddWithValue("@ban", tableId);
                object result = cmd.ExecuteScalar();

                return result == null || result == DBNull.Value
                    ? 0
                    : Convert.ToInt32(result);
            }
        }

        private void LoadOrder()
        {
            if (selectedTableId < 0)
                return;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                int billId = GetOpenBill(conn, selectedTableId);

                DataTable dt = new DataTable();

                if (billId > 0)
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(
                        @"SELECT ct.MaMon, m.TenMon,
                                 ct.DonGia, ct.SoLuong,
                                 ct.DonGia * ct.SoLuong AS ThanhTien
                          FROM ChiTietHoaDon ct
                          INNER JOIN MonAn m ON ct.MaMon = m.MaMon
                          WHERE ct.MaHoaDon = @id
                          ORDER BY m.TenMon", conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue(
                            "@id", billId);
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
                    total += Convert.ToDecimal(row["ThanhTien"]);

                lblTotal.Text = "Tổng tiền: " +
                    total.ToString("N0") + " đ";
            }
        }

        private void AddOrder()
        {
            if (selectedTableId < 0)
            {
                MessageBox.Show("Bạn hãy chọn bàn trước.");
                return;
            }

            if (cboProduct.SelectedValue == null ||
                cboProduct.SelectedValue is DataRowView)
            {
                MessageBox.Show("Danh mục này chưa có món ăn.");
                return;
            }

            int productId = Convert.ToInt32(cboProduct.SelectedValue);
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
                                cmd.Parameters.AddWithValue(
                                    "@ban", selectedTableId);
                                billId = Convert.ToInt32(
                                    cmd.ExecuteScalar());
                            }
                        }

                        decimal price;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT DonGia FROM MonAn WHERE MaMon=@mon",
                            conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@mon", productId);
                            object value = cmd.ExecuteScalar();

                            if (value == null || value == DBNull.Value)
                                throw new Exception("Không tìm thấy món ăn.");

                            price = Convert.ToDecimal(value);
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE ChiTietHoaDon
                              SET SoLuong = SoLuong + @sl
                              WHERE MaHoaDon = @hd AND MaMon = @mon",
                            conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@sl", quantity);
                            cmd.Parameters.AddWithValue("@hd", billId);
                            cmd.Parameters.AddWithValue("@mon", productId);

                            int affected = cmd.ExecuteNonQuery();

                            if (affected == 0)
                            {
                                using (SqlCommand insert = new SqlCommand(
                                    @"INSERT INTO ChiTietHoaDon
                                      (MaHoaDon, MaMon, SoLuong, DonGia)
                                      VALUES (@hd, @mon, @sl, @gia)",
                                    conn, tran))
                                {
                                    insert.Parameters.AddWithValue(
                                        "@hd", billId);
                                    insert.Parameters.AddWithValue(
                                        "@mon", productId);
                                    insert.Parameters.AddWithValue(
                                        "@sl", quantity);
                                    insert.Parameters.AddWithValue(
                                        "@gia", price);
                                    insert.ExecuteNonQuery();
                                }
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE BanAn SET TrangThai=N'Có khách'
                              WHERE MaBan=@ban", conn, tran))
                        {
                            cmd.Parameters.AddWithValue(
                                "@ban", selectedTableId);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }

            LoadTables();
            LoadOrder();
        }

        private void RemoveOrderItem()
        {
            if (selectedTableId < 0 || dgvOrder.CurrentRow == null)
            {
                MessageBox.Show("Hãy chọn bàn và dòng món cần xóa.");
                return;
            }

            int productId = Convert.ToInt32(
                dgvOrder.CurrentRow.Cells["MaMon"].Value);

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                int billId = GetOpenBill(conn, selectedTableId);
                if (billId == 0)
                    return;

                using (SqlCommand cmd = new SqlCommand(
                    @"DELETE FROM ChiTietHoaDon
                      WHERE MaHoaDon=@hd AND MaMon=@mon",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@hd", billId);
                    cmd.Parameters.AddWithValue("@mon", productId);
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
                              FROM ChiTietHoaDon WHERE MaHoaDon=@hd",
                            conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@hd", billId);
                            total = Convert.ToDecimal(cmd.ExecuteScalar());
                        }

                        if (total <= 0)
                        {
                            tran.Rollback();
                            MessageBox.Show("Hóa đơn chưa có món.");
                            return;
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE HoaDon
                              SET GioRa=GETDATE(), TongTien=@tien,
                                  TrangThai=1
                              WHERE MaHoaDon=@hd AND TrangThai=0",
                            conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@tien", total);
                            cmd.Parameters.AddWithValue("@hd", billId);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            @"UPDATE BanAn SET TrangThai=N'Trống'
                              WHERE MaBan=@ban", conn, tran))
                        {
                            cmd.Parameters.AddWithValue(
                                "@ban", selectedTableId);
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
                        MessageBox.Show("Lỗi thanh toán: " + ex.Message);
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

            decimal price;

            if (name.Length == 0)
            {
                MessageBox.Show("Tên món không được để trống.");
                txtName.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out price) ||
                price < 0)
            {
                MessageBox.Show("Đơn giá phải là số không âm.");
                txtPrice.Focus();
                return;
            }

            if (cboMenuCategory.SelectedValue == null ||
                cboMenuCategory.SelectedValue is DataRowView)
            {
                MessageBox.Show("Hãy chọn danh mục món.");
                return;
            }

            int categoryId = Convert.ToInt32(
                cboMenuCategory.SelectedValue);

            if (edit && selectedMenuId < 0)
            {
                MessageBox.Show("Hãy chọn món cần sửa trong bảng.");
                return;
            }

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();

                string sql = edit
                    ? @"UPDATE MonAn
                        SET TenMon=@ten, MaDanhMuc=@dm, DonGia=@gia
                        WHERE MaMon=@id"
                    : @"INSERT INTO MonAn(TenMon, MaDanhMuc, DonGia)
                        VALUES(@ten, @dm, @gia)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ten", name);
                    cmd.Parameters.AddWithValue("@dm", categoryId);
                    cmd.Parameters.AddWithValue("@gia", price);

                    if (edit)
                        cmd.Parameters.AddWithValue("@id", selectedMenuId);

                    cmd.ExecuteNonQuery();
                }
            }

            LoadMenuGrid();
            LoadProducts();
            ClearMenuInputs();

            MessageBox.Show(edit ? "Đã sửa món." : "Đã thêm món.");
        }

        private void DeleteMenu()
        {
            if (selectedMenuId < 0)
            {
                MessageBox.Show("Hãy chọn món cần xóa.");
                return;
            }

            if (MessageBox.Show(
                "Bạn có chắc muốn xóa món này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM MonAn WHERE MaMon=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedMenuId);
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
                    "Không thể xóa món đã được sử dụng trong hóa đơn. " +
                    "Hãy giữ lại món để bảo toàn lịch sử bán hàng.");
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
            dgvMenu.ClearSelection();
        }
    }
}
