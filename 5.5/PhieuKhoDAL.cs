
using System;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class PhieuKhoDAL
    {
        public int Save(PhieuKhoDTO p)
        {
            if (p == null || p.ChiTiet == null || p.ChiTiet.Count == 0)
                throw new Exception("Phiếu kho phải có ít nhất một vật tư.");

            if (p.LoaiPhieu != "NHAP" && p.LoaiPhieu != "XUAT")
                throw new Exception("Loại phiếu không hợp lệ.");

            if (p.LoaiPhieu == "NHAP" && !p.MaNCC.HasValue)
                throw new Exception("Phiếu nhập phải chọn nhà cung cấp.");

            foreach (ChiTietPhieuDTO ct in p.ChiTiet)
            {
                if (string.IsNullOrWhiteSpace(ct.MaVatTu) ||
                    ct.SoLuong <= 0 || ct.DonGia < 0)
                    throw new Exception("Chi tiết phiếu không hợp lệ.");
            }

            using (SqlConnection conn = Database.GetConnection())
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction(
                    IsolationLevel.Serializable))
                {
                    try
                    {
                        decimal tongTien = 0;

                        foreach (ChiTietPhieuDTO ct in p.ChiTiet)
                        {
                            tongTien += ct.SoLuong * ct.DonGia;
                        }

                        int maPhieu;

                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO PhieuKho
                              (LoaiPhieu, MaNCC, TongTien)
                              VALUES(@loai, @ncc, @tong);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);",
                            conn, trans))
                        {
                            cmd.Parameters.Add("@loai", SqlDbType.NVarChar, 10)
                                .Value = p.LoaiPhieu;

                            cmd.Parameters.Add("@ncc", SqlDbType.Int).Value =
                                p.LoaiPhieu == "NHAP"
                                ? (object)p.MaNCC.Value : DBNull.Value;

                            SqlParameter tong = cmd.Parameters.Add(
                                "@tong", SqlDbType.Decimal);
                            tong.Precision = 18;
                            tong.Scale = 2;
                            tong.Value = tongTien;

                            maPhieu = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (ChiTietPhieuDTO ct in p.ChiTiet)
                        {
                            using (SqlCommand cmd = new SqlCommand(
                                @"INSERT INTO ChiTietPhieuKho
                                  (MaPhieu, MaVatTu, SoLuong, DonGia)
                                  VALUES(@phieu, @vatTu, @sl, @gia)",
                                conn, trans))
                            {
                                cmd.Parameters.Add("@phieu", SqlDbType.Int)
                                    .Value = maPhieu;
                                cmd.Parameters.Add("@vatTu", SqlDbType.VarChar, 20)
                                    .Value = ct.MaVatTu;

                                SqlParameter sl = cmd.Parameters.Add(
                                    "@sl", SqlDbType.Decimal);
                                sl.Precision = 18;
                                sl.Scale = 2;
                                sl.Value = ct.SoLuong;

                                SqlParameter gia = cmd.Parameters.Add(
                                    "@gia", SqlDbType.Decimal);
                                gia.Precision = 18;
                                gia.Scale = 2;
                                gia.Value = ct.DonGia;

                                cmd.ExecuteNonQuery();
                            }

                            string sql = p.LoaiPhieu == "NHAP"
                                ? @"UPDATE VatTu
                                    SET SoLuongTon = SoLuongTon + @sl
                                    WHERE MaVatTu = @ma"
                                : @"UPDATE VatTu
                                    SET SoLuongTon = SoLuongTon - @sl
                                    WHERE MaVatTu = @ma
                                    AND SoLuongTon >= @sl";

                            using (SqlCommand cmd = new SqlCommand(
                                sql, conn, trans))
                            {
                                cmd.Parameters.Add("@sl", SqlDbType.Decimal)
                                    .Value = ct.SoLuong;
                                cmd.Parameters["@sl"].Precision = 18;
                                cmd.Parameters["@sl"].Scale = 2;

                                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20)
                                    .Value = ct.MaVatTu;

                                int affected = cmd.ExecuteNonQuery();

                                if (affected == 0)
                                {
                                    using (SqlCommand check = new SqlCommand(
                                        "SELECT COUNT(*) FROM VatTu WHERE MaVatTu=@ma",
                                        conn, trans))
                                    {
                                        check.Parameters.Add("@ma",
                                            SqlDbType.VarChar, 20).Value = ct.MaVatTu;

                                        int exists = Convert.ToInt32(
                                            check.ExecuteScalar());

                                        if (exists == 0)
                                            throw new Exception(
                                                "Không tìm thấy vật tư: " + ct.MaVatTu);

                                        throw new Exception(
                                            "Không đủ hàng trong kho");
                                    }
                                }
                            }
                        }

                        trans.Commit();
                        return maPhieu;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
