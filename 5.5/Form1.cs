
using System;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using InventoryManagement.DTO;
using InventoryManagement.BUS;

namespace InventoryManagement
{
    public partial class Form1 : Form
    {
        private readonly VatTuBUS vatTuBUS = new VatTuBUS();
        private readonly NhaCungCapBUS nccBUS = new NhaCungCapBUS();
        private readonly PhieuKhoBUS phieuBUS = new PhieuKhoBUS();

        private DataGridView dgvVatTu;
        private DataGridView dgvNCC;
        private DataGridView dgvChiTiet;
        private DataGridView dgvCanhBao;

        private TextBox txtMaVT, txtTenVT, txtDVT, txtTon, txtMuc, txtTim;
        private TextBox txtTenNCC, txtSDT, txtDiaChi;
        private ComboBox cboLoai, cboNCC, cboVatTu;
        private NumericUpDown numSL, numGia;
        private Label lblTong, lblGiaTri;

        private int maNCCChon;
        private readonly List<ChiTietPhieuDTO> chiTiet =
            new List<ChiTietPhieuDTO>();

        public Form1()
        {
            InitializeComponent();
            TaoGiaoDien();
            NapDuLieu();
        }

        private void TaoGiaoDien()
        {
            Text = "HỆ THỐNG QUẢN LÝ KHO NGUYÊN VẬT LIỆU";
            Size = new Size(1200, 750);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            Controls.Add(tabs);

            TabPage tabVT = new TabPage("Quản lý vật tư");
            TabPage tabNCC = new TabPage("Nhà cung cấp");
            TabPage tabPhieu = new TabPage("Phiếu nhập / xuất");
            TabPage tabBC = new TabPage("Báo cáo tồn kho");

            tabs.TabPages.AddRange(new[]
            {
                tabVT, tabNCC, tabPhieu, tabBC
            });

            TaoTabVatTu(tabVT);
            TaoTabNCC(tabNCC);
            TaoTabPhieu(tabPhieu);
            TaoTabBaoCao(tabBC);
        }

        private Button Nut(string text, Action action)
        {
            Button b = new Button();
            b.Text = text;
            b.AutoSize = true;
            b.Padding = new Padding(6);
            b.Click += (s, e) =>
            {
                try { action(); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            return b;
        }

        private DataGridView TaoBang()
        {
            DataGridView d = new DataGridView();
            d.Dock = DockStyle.Fill;
            d.ReadOnly = true;
            d.AllowUserToAddRows = false;
            d.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            d.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            d.MultiSelect = false;
            return d;
        }

        private void TaoTabVatTu(TabPage tab)
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(root);

            FlowLayoutPanel input = new FlowLayoutPanel();
            input.Dock = DockStyle.Fill;
            input.AutoScroll = true;

            txtMaVT = new TextBox { Width = 110 };
            txtTenVT = new TextBox { Width = 150 };
            txtDVT = new TextBox { Width = 80 };
            txtTon = new TextBox { Width = 80, Text = "0" };
            txtMuc = new TextBox { Width = 80, Text = "10" };
            txtTim = new TextBox { Width = 140 };

            input.Controls.AddRange(new Control[]
            {
                new Label { Text = "Mã vật tư", AutoSize = true },
                txtMaVT,
                new Label { Text = "Tên vật tư", AutoSize = true },
                txtTenVT,
                new Label { Text = "Đơn vị", AutoSize = true },
                txtDVT,
                new Label { Text = "Tồn ban đầu", AutoSize = true },
                txtTon,
                new Label { Text = "Mức an toàn", AutoSize = true },
                txtMuc
            });

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;

            buttons.Controls.Add(Nut("Thêm", () =>
            {
                VatTuDTO v = LayVatTu();
                vatTuBUS.Insert(v);
                NapVatTu();
                MessageBox.Show("Thêm vật tư thành công.");
            }));

            buttons.Controls.Add(Nut("Sửa", () =>
            {
                VatTuDTO v = LayVatTu();
                vatTuBUS.Update(v);
                NapVatTu();
                MessageBox.Show("Cập nhật thành công.");
            }));

            buttons.Controls.Add(Nut("Xóa", () =>
            {
                vatTuBUS.Delete(txtMaVT.Text.Trim());
                NapVatTu();
                MessageBox.Show("Đã xóa vật tư.");
            }));

            buttons.Controls.Add(new Label
            {
                Text = "Tìm kiếm",
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            });
            buttons.Controls.Add(txtTim);
            buttons.Controls.Add(Nut("Tìm", () =>
            {
                dgvVatTu.DataSource = vatTuBUS.Search(txtTim.Text.Trim());
            }));
            buttons.Controls.Add(Nut("Tải lại", NapVatTu));

            dgvVatTu = TaoBang();
            dgvVatTu.SelectionChanged += (s, e) =>
            {
                if (dgvVatTu.CurrentRow == null) return;
                DataGridViewRow r = dgvVatTu.CurrentRow;
                if (r.Cells["MaVatTu"].Value == null) return;

                txtMaVT.Text = Convert.ToString(r.Cells["MaVatTu"].Value);
                txtTenVT.Text = Convert.ToString(r.Cells["TenVatTu"].Value);
                txtDVT.Text = Convert.ToString(r.Cells["DVT"].Value);
                txtTon.Text = Convert.ToString(r.Cells["SoLuongTon"].Value);
                txtMuc.Text = Convert.ToString(r.Cells["MucTonAnToan"].Value);
                txtMaVT.ReadOnly = true;
            };

            root.Controls.Add(input, 0, 0);
            root.Controls.Add(buttons, 0, 1);
            root.Controls.Add(dgvVatTu, 0, 2);
        }

        private VatTuDTO LayVatTu()
        {
            decimal ton, muc;

            if (!decimal.TryParse(txtTon.Text, out ton) || ton < 0)
                throw new Exception("Số lượng tồn ban đầu không hợp lệ.");

            if (!decimal.TryParse(txtMuc.Text, out muc) || muc < 0)
                throw new Exception("Mức tồn an toàn không hợp lệ.");

            return new VatTuDTO
            {
                MaVatTu = txtMaVT.Text.Trim(),
                TenVatTu = txtTenVT.Text.Trim(),
                DVT = txtDVT.Text.Trim(),
                SoLuongTon = ton,
                MucTonAnToan = muc
            };
        }

        private void TaoTabNCC(TabPage tab)
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(root);

            FlowLayoutPanel input = new FlowLayoutPanel();
            input.Dock = DockStyle.Fill;
            input.AutoScroll = true;

            TextBox txtMa = new TextBox { Width = 70, ReadOnly = true };
            txtTenNCC = new TextBox { Width = 180 };
            txtSDT = new TextBox { Width = 120 };
            txtDiaChi = new TextBox { Width = 200 };

            input.Controls.AddRange(new Control[]
            {
                new Label { Text = "Mã NCC", AutoSize = true }, txtMa,
                new Label { Text = "Tên NCC", AutoSize = true }, txtTenNCC,
                new Label { Text = "Điện thoại", AutoSize = true }, txtSDT,
                new Label { Text = "Địa chỉ", AutoSize = true }, txtDiaChi
            });

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;

            buttons.Controls.Add(Nut("Thêm NCC", () =>
            {
                nccBUS.Insert(LayNCC());
                NapNCC();
                MessageBox.Show("Đã thêm nhà cung cấp.");
            }));

            buttons.Controls.Add(Nut("Sửa NCC", () =>
            {
                NhaCungCapDTO n = LayNCC();
                n.MaNCC = maNCCChon;
                nccBUS.Update(n);
                NapNCC();
                MessageBox.Show("Đã cập nhật nhà cung cấp.");
            }));

            buttons.Controls.Add(Nut("Xóa NCC", () =>
            {
                if (maNCCChon <= 0)
                    throw new Exception("Hãy chọn nhà cung cấp.");

                nccBUS.Delete(maNCCChon);
                maNCCChon = 0;
                NapNCC();
            }));

            dgvNCC = TaoBang();
            dgvNCC.SelectionChanged += (s, e) =>
            {
                if (dgvNCC.CurrentRow == null) return;
                DataGridViewRow r = dgvNCC.CurrentRow;
                if (r.Cells["MaNCC"].Value == null) return;

                maNCCChon = Convert.ToInt32(r.Cells["MaNCC"].Value);
                txtMa.Text = maNCCChon.ToString();
                txtTenNCC.Text = Convert.ToString(r.Cells["TenNCC"].Value);
                txtSDT.Text = Convert.ToString(r.Cells["DienThoai"].Value);
                txtDiaChi.Text = Convert.ToString(r.Cells["DiaChi"].Value);
            };

            root.Controls.Add(input, 0, 0);
            root.Controls.Add(buttons, 0, 1);
            root.Controls.Add(dgvNCC, 0, 2);
        }

        private NhaCungCapDTO LayNCC()
        {
            return new NhaCungCapDTO
            {
                TenNCC = txtTenNCC.Text.Trim(),
                DienThoai = txtSDT.Text.Trim(),
                DiaChi = txtDiaChi.Text.Trim()
            };
        }

        private void TaoTabPhieu(TabPage tab)
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(root);

            FlowLayoutPanel top = new FlowLayoutPanel();
            top.Dock = DockStyle.Fill;

            cboLoai = new ComboBox
            {
                Width = 100,
                DropDownStyle =
                ComboBoxStyle.DropDownList
            };
            cboLoai.Items.AddRange(new object[] { "NHAP", "XUAT" });
            cboLoai.SelectedIndex = 0;
            cboLoai.SelectedIndexChanged += (s, e) =>
            {
                if (cboNCC != null) cboNCC.Enabled = cboLoai.Text == "NHAP";
            };

            cboNCC = new ComboBox
            {
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "TenNCC",
                ValueMember = "MaNCC"
            };

            cboVatTu = new ComboBox
            {
                Width = 230,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "TenVatTu",
                ValueMember = "MaVatTu"
            };

            numSL = new NumericUpDown
            {
                Width = 100,
                DecimalPlaces = 2,
                Minimum = 0.01m,
                Maximum = 100000000
            };

            numGia = new NumericUpDown
            {
                Width = 120,
                DecimalPlaces = 2,
                Minimum = 0,
                Maximum = 1000000000
            };

            top.Controls.AddRange(new Control[]
            {
                new Label { Text = "Loại phiếu", AutoSize = true }, cboLoai,
                new Label { Text = "Nhà cung cấp", AutoSize = true }, cboNCC,
                new Label { Text = "Vật tư", AutoSize = true }, cboVatTu,
                new Label { Text = "Số lượng", AutoSize = true }, numSL,
                new Label { Text = "Đơn giá", AutoSize = true }, numGia
            });

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            lblTong = new Label
            {
                Text = "Tổng tiền: 0",
                AutoSize = true,
                Padding = new Padding(5, 10, 5, 0)
            };

            buttons.Controls.Add(Nut("Thêm vật tư vào phiếu", ThemChiTiet));
            buttons.Controls.Add(Nut("Xóa dòng", XoaChiTiet));
            buttons.Controls.Add(Nut("Lưu phiếu kho", LuuPhieu));
            buttons.Controls.Add(lblTong);

            dgvChiTiet = TaoBang();
            dgvChiTiet.DataSource = TaoBangChiTiet();

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(buttons, 0, 1);
            root.Controls.Add(dgvChiTiet, 0, 2);
        }

        private DataTable TaoBangChiTiet()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MaVatTu", typeof(string));
            dt.Columns.Add("TenVatTu", typeof(string));
            dt.Columns.Add("SoLuong", typeof(decimal));
            dt.Columns.Add("DonGia", typeof(decimal));
            dt.Columns.Add("ThanhTien", typeof(decimal));
            return dt;
        }

        private void ThemChiTiet()
        {
            if (cboVatTu.SelectedValue == null)
                throw new Exception("Hãy chọn vật tư.");

            string ma = cboVatTu.SelectedValue.ToString();

            foreach (ChiTietPhieuDTO ct in chiTiet)
                if (ct.MaVatTu == ma)
                    throw new Exception("Vật tư đã có trong phiếu.");

            decimal sl = numSL.Value;
            decimal gia = numGia.Value;

            chiTiet.Add(new ChiTietPhieuDTO
            {
                MaVatTu = ma,
                SoLuong = sl,
                DonGia = gia
            });

            DataTable dt = TaoBangChiTiet();
            decimal tong = 0;

            foreach (ChiTietPhieuDTO ct in chiTiet)
            {
                string ten = "";
                DataRowView item = cboVatTu.Items.Count > 0
                    ? cboVatTu.SelectedItem as DataRowView : null;

                if (item != null && item["MaVatTu"].ToString() == ct.MaVatTu)
                    ten = item["TenVatTu"].ToString();

                decimal thanhTien = ct.SoLuong * ct.DonGia;
                tong += thanhTien;
                dt.Rows.Add(ct.MaVatTu, ten, ct.SoLuong, ct.DonGia, thanhTien);
            }

            dgvChiTiet.DataSource = dt;
            lblTong.Text = "Tổng tiền: " + tong.ToString("N2");
        }

        private void XoaChiTiet()
        {
            if (dgvChiTiet.CurrentRow == null)
                throw new Exception("Hãy chọn dòng cần xóa.");

            string ma = Convert.ToString(
                dgvChiTiet.CurrentRow.Cells["MaVatTu"].Value);

            chiTiet.RemoveAll(x => x.MaVatTu == ma);
            DataTable dt = TaoBangChiTiet();
            decimal tong = 0;

            foreach (ChiTietPhieuDTO ct in chiTiet)
            {
                decimal tien = ct.SoLuong * ct.DonGia;
                tong += tien;
                dt.Rows.Add(ct.MaVatTu, ct.MaVatTu, ct.SoLuong,
                    ct.DonGia, tien);
            }

            dgvChiTiet.DataSource = dt;
            lblTong.Text = "Tổng tiền: " + tong.ToString("N2");
        }

        private void LuuPhieu()
        {
            if (chiTiet.Count == 0)
                throw new Exception("Hãy thêm vật tư vào phiếu.");

            int? ncc = null;
            if (cboLoai.Text == "NHAP")
            {
                if (cboNCC.SelectedValue == null)
                    throw new Exception("Hãy chọn nhà cung cấp.");

                ncc = Convert.ToInt32(cboNCC.SelectedValue);
            }

            PhieuKhoDTO p = new PhieuKhoDTO
            {
                LoaiPhieu = cboLoai.Text,
                MaNCC = ncc,
                NgayLap = DateTime.Now,
                ChiTiet = new List<ChiTietPhieuDTO>(chiTiet)
            };

            int maPhieu = phieuBUS.Save(p);

            MessageBox.Show("Lưu phiếu thành công. Mã phiếu: " + maPhieu);

            chiTiet.Clear();
            dgvChiTiet.DataSource = TaoBangChiTiet();
            lblTong.Text = "Tổng tiền: 0";
            NapDuLieu();
        }

        private void TaoTabBaoCao(TabPage tab)
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 3;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tab.Controls.Add(root);

            FlowLayoutPanel top = new FlowLayoutPanel();
            top.Dock = DockStyle.Fill;

            lblGiaTri = new Label
            {
                Text = "Tổng giá trị tồn kho: 0",
                AutoSize = true,
                Padding = new Padding(5, 10, 5, 0)
            };

            top.Controls.Add(lblGiaTri);
            top.Controls.Add(Nut("Tính giá trị tồn kho", () =>
            {
                decimal giaTri = vatTuBUS.GetInventoryValue();
                lblGiaTri.Text = "Tổng giá trị tồn kho: " +
                    giaTri.ToString("N2");
            }));

            FlowLayoutPanel buttons = new FlowLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.Controls.Add(Nut("Cảnh báo tồn kho thấp", NapCanhBao));
            buttons.Controls.Add(Nut("Tải lại tất cả", NapDuLieu));

            dgvCanhBao = TaoBang();
            root.Controls.Add(top, 0, 0);
            root.Controls.Add(buttons, 0, 1);
            root.Controls.Add(dgvCanhBao, 0, 2);
        }

        private void NapVatTu()
        {
            dgvVatTu.DataSource = vatTuBUS.GetAll();
            txtMaVT.ReadOnly = false;
            txtMaVT.Clear();
            txtTenVT.Clear();
            txtDVT.Clear();
            txtTon.Text = "0";
            txtMuc.Text = "10";
        }

        private void NapNCC()
        {
            dgvNCC.DataSource = nccBUS.GetAll();
            cboNCC.DataSource = nccBUS.GetAll();
            cboNCC.DisplayMember = "TenNCC";
            cboNCC.ValueMember = "MaNCC";
        }

        private void NapCanhBao()
        {
            dgvCanhBao.DataSource = vatTuBUS.GetLowStock();
        }

        private void NapDuLieu()
        {
            if (dgvVatTu != null) NapVatTu();
            if (dgvNCC != null) NapNCC();

            if (cboVatTu != null)
            {
                cboVatTu.DataSource = vatTuBUS.GetAll();
                cboVatTu.DisplayMember = "TenVatTu";
                cboVatTu.ValueMember = "MaVatTu";
            }

            if (dgvCanhBao != null) NapCanhBao();
        }
    }
}
