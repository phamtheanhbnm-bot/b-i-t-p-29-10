
using System;
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class VatTuDAL
    {
        public DataTable GetAll()
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM VatTu ORDER BY MaVatTu", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable Search(string keyword)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT * FROM VatTu WHERE MaVatTu LIKE @kw OR TenVatTu LIKE @kw", conn))
            {
                cmd.Parameters.Add("@kw", SqlDbType.NVarChar, 150).Value =
                    "%" + keyword + "%";

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public int Insert(VatTuDTO v)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO VatTu
                (MaVatTu, TenVatTu, DVT, SoLuongTon, MucTonAnToan)
                VALUES (@ma, @ten, @dvt, @ton, @muc)", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = v.MaVatTu;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 150).Value = v.TenVatTu;
                cmd.Parameters.Add("@dvt", SqlDbType.NVarChar, 20).Value =
                    (object)v.DVT ?? DBNull.Value;
                cmd.Parameters.Add("@ton", SqlDbType.Decimal).Value = v.SoLuongTon;
                cmd.Parameters["@ton"].Precision = 18;
                cmd.Parameters["@ton"].Scale = 2;
                cmd.Parameters.Add("@muc", SqlDbType.Decimal).Value = v.MucTonAnToan;
                cmd.Parameters["@muc"].Precision = 18;
                cmd.Parameters["@muc"].Scale = 2;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(VatTuDTO v)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"UPDATE VatTu SET TenVatTu=@ten, DVT=@dvt,
                MucTonAnToan=@muc WHERE MaVatTu=@ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = v.MaVatTu;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 150).Value = v.TenVatTu;
                cmd.Parameters.Add("@dvt", SqlDbType.NVarChar, 20).Value =
                    (object)v.DVT ?? DBNull.Value;
                cmd.Parameters.Add("@muc", SqlDbType.Decimal).Value = v.MucTonAnToan;
                cmd.Parameters["@muc"].Precision = 18;
                cmd.Parameters["@muc"].Scale = 2;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(string ma)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM VatTu WHERE MaVatTu=@ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.VarChar, 20).Value = ma;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetLowStock()
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                @"SELECT MaVatTu, TenVatTu, DVT, SoLuongTon, MucTonAnToan
                  FROM VatTu WHERE SoLuongTon < MucTonAnToan
                  ORDER BY SoLuongTon", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public decimal GetInventoryValue()
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT ISNULL(SUM(SoLuongTon * GiaNhap.Gia), 0)
                  FROM VatTu v
                  OUTER APPLY
                  (
                      SELECT TOP 1 DonGia AS Gia
                      FROM ChiTietPhieuKho ct
                      INNER JOIN PhieuKho p ON p.MaPhieu = ct.MaPhieu
                      WHERE ct.MaVatTu = v.MaVatTu AND p.LoaiPhieu = N'NHAP'
                      ORDER BY p.NgayLap DESC, p.MaPhieu DESC
                  ) GiaNhap", conn))
            {
                conn.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }
}
