using System.Data;
using System.Drawing;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace HotelManagement
{
    public partial class Form1 : Form
    {
        private readonly BUS bus = new BUS();
        private readonly BindingSource roomSource = new BindingSource();
        private readonly DataTable roomTable = new DataTable();

        private DataGridView dgvRooms = new DataGridView();
        private DataGridView dgvBookings = new DataGridView();
        private DataGridView dgvServicesUsed = new DataGridView();

        private TextBox txtName = new TextBox();
        private TextBox txtCCCD = new TextBox();
        private TextBox txtPhone = new TextBox();

        private ComboBox cboRoom = new ComboBox();
        private ComboBox cboService = new ComboBox();
        private ComboBox cboBooking = new ComboBox();
        private ComboBox cboType = new ComboBox();
        private ComboBox cboStatus = new ComboBox();

        private NumericUpDown numQuantity = new NumericUpDown();
        private Label lblTotal = new Label();

        public Form1()
        {
            InitializeComponent();

            Text = "HOTEL MANAGEMENT SYSTEM";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 750);
            MinimumSize = new Size(950, 600);
            Font = new Font("Segoe UI", 10);
            BuildInterface();
            LoadAllData();
        }

        private Button CreateButton(string text, EventHandler click)
        {
            Button button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 36,
                Padding = new Padding(8, 3, 8, 3),
                Margin = new Padding(5)
            };

            button.Click += click;
            return button;
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Width = 230,
                Margin = new Padding(5)
            };
        }

        private ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(5)
            };
        }

        private void AddField(
            TableLayoutPanel panel, string label, Control control)
        {
            int row = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            panel.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(5, 9, 5, 5)
            }, 0, row);

            panel.Controls.Add(control, 1, row);
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };
        }

        private TabPage CreateTab(string title)
        {
            return new TabPage(title) { Padding = new Padding(10) };
        }

        private void BuildInterface()
        {
            TabControl tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            Controls.Add(tabs);

            TabPage tabCheckIn = CreateTab("Check-in");
            TabPage tabService = CreateTab("Gọi dịch vụ");
            TabPage tabCheckOut = CreateTab("Check-out / Hóa đơn");
            TabPage tabRooms = CreateTab("Quản lý phòng");

            tabs.TabPages.AddRange(new[]
            {
                tabCheckIn, tabService, tabCheckOut, tabRooms
            });

            BuildCheckInTab(tabCheckIn);
            BuildServiceTab(tabService);
            BuildCheckOutTab(tabCheckOut);
            BuildRoomsTab(tabRooms);
        }

        private void BuildCheckInTab(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 0
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            txtName = CreateTextBox();
            txtCCCD = CreateTextBox();
            txtPhone = CreateTextBox();
            cboRoom = CreateComboBox();

            AddField(layout, "Họ tên khách hàng:", txtName);
            AddField(layout, "CCCD:", txtCCCD);
            AddField(layout, "Số điện thoại:", txtPhone);
            AddField(layout, "Phòng còn trống:", cboRoom);

            FlowLayoutPanel buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };

            buttons.Controls.Add(CreateButton("Tra cứu CCCD", (s, e) =>
            {
                try
                {
                    DataTable dt = bus.SearchCustomer(txtCCCD.Text.Trim());

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("Khách hàng mới.");
                        return;
                    }

                    txtName.Text = dt.Rows[0]["HoTen"].ToString();
                    txtPhone.Text = dt.Rows[0]["SoDienThoai"].ToString();
                    MessageBox.Show("Đã tìm thấy khách hàng.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));

            buttons.Controls.Add(CreateButton("Nhận phòng", (s, e) =>
            {
                try
                {
                    if (cboRoom.SelectedValue == null)
                        throw new Exception("Vui lòng chọn phòng trống.");

                    int maKH = bus.CheckIn(
                        txtName.Text,
                        txtCCCD.Text,
                        txtPhone.Text,
                        cboRoom.SelectedValue.ToString()!);

                    MessageBox.Show(
                        $"Nhận phòng thành công!\nMã khách hàng: {maKH}");

                    LoadAllData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Thông báo");
                }
            }));

            tab.Controls.Add(buttons);
            tab.Controls.Add(layout);
        }

        private void BuildServiceTab(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            cboBooking = CreateComboBox();
            cboService = CreateComboBox();

            numQuantity = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 1000,
                Value = 1,
                Width = 230,
                Margin = new Padding(5)
            };

            AddField(layout, "Phiếu đang ở:", cboBooking);
            AddField(layout, "Dịch vụ:", cboService);
            AddField(layout, "Số lượng:", numQuantity);

            Button btnAdd = CreateButton("Gọi dịch vụ", (s, e) =>
            {
                try
                {
                    if (cboBooking.SelectedValue == null ||
                        cboService.SelectedValue == null)
                        throw new Exception("Vui lòng chọn phiếu và dịch vụ.");

                    bus.AddService(
                        Convert.ToInt32(cboBooking.SelectedValue),
                        Convert.ToInt32(cboService.SelectedValue),
                        (int)numQuantity.Value);

                    MessageBox.Show("Đã thêm dịch vụ.");
                    LoadAllData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            tab.Controls.Add(btnAdd);
            tab.Controls.Add(layout);

            dgvServicesUsed = CreateGrid();
            dgvServicesUsed.Height = 350;
            dgvServicesUsed.Dock = DockStyle.Bottom;
            tab.Controls.Add(dgvServicesUsed);
        }

        private void BuildCheckOutTab(TabPage tab)
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            ComboBox cboCheckOut = CreateComboBox();

            AddField(layout, "Phiếu thanh toán:", cboCheckOut);

            lblTotal = new Label
            {
                Text = "Tổng thanh toán: 0 VNĐ",
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Margin = new Padding(5, 10, 5, 10)
            };

            Button btnPay = CreateButton("Xác nhận thanh toán", (s, e) =>
            {
                try
                {
                    if (cboCheckOut.SelectedValue == null)
                        throw new Exception("Vui lòng chọn phiếu.");

                    int maPhieu = Convert.ToInt32(cboCheckOut.SelectedValue);

                    DialogResult confirm = MessageBox.Show(
                        "Xác nhận thanh toán và trả phòng?",
                        "Thanh toán",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes)
                        return;

                    decimal total = bus.CheckOut(maPhieu);

                    lblTotal.Text = "Tổng thanh toán: " +
                        total.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) +
                        " VNĐ";

                    MessageBox.Show(
                        $"THANH TOÁN THÀNH CÔNG\n" +
                        $"Mã phiếu: {maPhieu}\n" +
                        $"Tổng tiền: {total:N0} VNĐ\n" +
                        $"Trạng thái phòng: Đang dọn dẹp");

                    LoadAllData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });

            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };

            panel.Controls.Add(btnPay);
            panel.Controls.Add(lblTotal);

            dgvBookings = CreateGrid();
            dgvBookings.Dock = DockStyle.Fill;

            tab.Controls.Add(dgvBookings);
            tab.Controls.Add(panel);
            tab.Controls.Add(layout);

            cboCheckOut.SelectedIndexChanged += (s, e) =>
            {
                if (cboCheckOut.SelectedValue == null ||
                    cboCheckOut.SelectedValue is DataRowView)
                    return;

                try
                {
                    int id = Convert.ToInt32(cboCheckOut.SelectedValue);
                    dgvBookings.DataSource = bus.GetBookingDetails(id);
                }
                catch
                {
                    dgvBookings.DataSource = null;
                }
            };

            cboCheckOut.Tag = "CHECKOUT";
            tab.Tag = cboCheckOut;
        }

        private void BuildRoomsTab(TabPage tab)
        {
            TableLayoutPanel filters = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4
            };

            cboType = CreateComboBox();
            cboStatus = CreateComboBox();

            cboType.Items.AddRange(new object[]
            {
                "Tất cả", "Đơn", "Đôi", "VIP"
            });

            cboStatus.Items.AddRange(new object[]
            {
                "Tất cả", "Trống", "Đang ở", "Đang dọn dẹp"
            });

            cboType.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;

            filters.Controls.Add(new Label
            {
                Text = "Loại phòng:",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 0, 0);

            filters.Controls.Add(cboType, 1, 0);

            filters.Controls.Add(new Label
            {
                Text = "Trạng thái:",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            }, 2, 0);

            filters.Controls.Add(cboStatus, 3, 0);

            cboType.SelectedIndexChanged += (s, e) => ApplyRoomFilter();
            cboStatus.SelectedIndexChanged += (s, e) => ApplyRoomFilter();

            dgvRooms = CreateGrid();
            dgvRooms.DataSource = roomSource;

            tab.Controls.Add(dgvRooms);
            tab.Controls.Add(filters);
        }

        private void LoadAllData()
        {
            try
            {
                DataTable rooms = bus.GetRooms();

                roomTable.Clear();
                roomTable.Merge(rooms);
                roomSource.DataSource = roomTable.DefaultView;

                DataTable available = bus.GetAvailableRooms();
                cboRoom.DataSource = available;
                cboRoom.DisplayMember = "SoPhong";
                cboRoom.ValueMember = "SoPhong";

                DataTable services = bus.GetServices();
                cboService.DataSource = services;
                cboService.DisplayMember = "TenDV";
                cboService.ValueMember = "MaDV";

                DataTable bookings = bus.GetActiveBookings();

                DataTable serviceBookings = bookings.Copy();
                cboBooking.DataSource = serviceBookings;
                cboBooking.DisplayMember = "MaPhieu";
                cboBooking.ValueMember = "MaPhieu";

                Control? checkoutControl = Controls
                    .Find("", true)
                    .FirstOrDefault();

                TabControl? tabs = Controls.OfType<TabControl>().FirstOrDefault();

                if (tabs != null)
                {
                    foreach (TabPage page in tabs.TabPages)
                    {
                        if (page.Tag is ComboBox checkout)
                        {
                            DataTable checkoutBookings = bookings.Copy();
                            checkout.DataSource = checkoutBookings;
                            checkout.DisplayMember = "MaPhieu";
                            checkout.ValueMember = "MaPhieu";
                        }
                    }
                }

                if (cboBooking.SelectedValue != null &&
                    !(cboBooking.SelectedValue is DataRowView))
                {
                    dgvServicesUsed.DataSource = bus.GetBookingDetails(
                        Convert.ToInt32(cboBooking.SelectedValue));
                }

                ApplyRoomFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể tải dữ liệu. Hãy kiểm tra SQL Server và CSDL HotelDB.\n" +
                    ex.Message);
            }
        }

        private void ApplyRoomFilter()
        {
            if (roomTable.Columns.Count == 0)
                return;

            List<string> conditions = new List<string>();

            if (cboType.SelectedItem?.ToString() is string type &&
                type != "Tất cả")
            {
                conditions.Add("LoaiPhong = '" + type.Replace("'", "''") + "'");
            }

            if (cboStatus.SelectedItem?.ToString() is string status &&
                status != "Tất cả")
            {
                conditions.Add("TrangThai = '" + status.Replace("'", "''") + "'");
            }

            roomSource.Filter = conditions.Count == 0
                ? ""
                : string.Join(" AND ", conditions);
        }
    }
}