using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelManagement
{
    public class DAL
    {
        private readonly string connectionString =
    @"Server=ThếAnhSIUVIPPROMAX;Database=HotelDB;Integrated Security=True;TrustServerCertificate=True;";

        private SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public DataTable GetData(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection conn = GetConnection();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public int Execute(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public object? Scalar(string sql, params SqlParameter[] parameters)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();
            using SqlCommand cmd = new SqlCommand(sql, conn);
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public int CheckIn(
            string hoTen, string cccd, string sdt, string soPhong)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            try
            {
                int maKH;

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT MaKH FROM KhachHang WHERE CCCD=@CCCD",
                    conn, tran))
                {
                    cmd.Parameters.Add("@CCCD", SqlDbType.VarChar, 20).Value = cccd;
                    object? result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        maKH = Convert.ToInt32(result);

                        using SqlCommand update = new SqlCommand(
                            "UPDATE KhachHang SET HoTen=@HoTen, SoDienThoai=@SDT WHERE MaKH=@MaKH",
                            conn, tran);
                        update.Parameters.AddWithValue("@HoTen", hoTen);
                        update.Parameters.AddWithValue("@SDT", sdt);
                        update.Parameters.AddWithValue("@MaKH", maKH);
                        update.ExecuteNonQuery();
                    }
                    else
                    {
                        using SqlCommand insert = new SqlCommand(
                            @"INSERT INTO KhachHang(HoTen,CCCD,SoDienThoai)
                              VALUES(@HoTen,@CCCD,@SDT);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);",
                            conn, tran);

                        insert.Parameters.AddWithValue("@HoTen", hoTen);
                        insert.Parameters.Add("@CCCD", SqlDbType.VarChar, 20).Value = cccd;
                        insert.Parameters.AddWithValue("@SDT", sdt);
                        maKH = Convert.ToInt32(insert.ExecuteScalar());
                    }
                }

                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Phong SET TrangThai=N'Đang ở' WHERE SoPhong=@SoPhong AND TrangThai=N'Trống'",
                    conn, tran))
                {
                    cmd.Parameters.AddWithValue("@SoPhong", soPhong);

                    if (cmd.ExecuteNonQuery() != 1)
                        throw new Exception("Phòng không còn trống.");
                }

                using (SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO PhieuDatPhong(SoPhong,MaKH,NgayNhanPhong)
                      VALUES(@SoPhong,@MaKH,GETDATE())",
                    conn, tran))
                {
                    cmd.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmd.Parameters.AddWithValue("@MaKH", maKH);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return maKH;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void AddService(int maPhieu, int maDV, int soLuong)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();

            using SqlTransaction tran = conn.BeginTransaction();

            try
            {
                using SqlCommand check = new SqlCommand(
                    @"SELECT COUNT(*) FROM PhieuDatPhong
                      WHERE MaPhieu=@MaPhieu AND NgayTraPhong IS NULL",
                    conn, tran);

                check.Parameters.AddWithValue("@MaPhieu", maPhieu);

                if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                    throw new Exception("Phiếu không tồn tại hoặc đã trả phòng.");

                using SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO SuDungDichVu(MaPhieu,MaDV,SoLuong,ThoiGian)
                      VALUES(@MaPhieu,@MaDV,@SoLuong,GETDATE())",
                    conn, tran);

                cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                cmd.Parameters.AddWithValue("@MaDV", maDV);
                cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                cmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public decimal CheckOut(int maPhieu)
        {
            using SqlConnection conn = GetConnection();
            conn.Open();
            using SqlTransaction tran = conn.BeginTransaction();

            try
            {
                string soPhong;
                DateTime ngayNhan;

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT SoPhong,NgayNhanPhong FROM PhieuDatPhong
                      WHERE MaPhieu=@MaPhieu AND NgayTraPhong IS NULL",
                    conn, tran))
                {
                    cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                    using SqlDataReader reader = cmd.ExecuteReader();

                    if (!reader.Read())
                        throw new Exception("Phiếu không tồn tại hoặc đã thanh toán.");

                    soPhong = reader.GetString(0);
                    ngayNhan = reader.GetDateTime(1);
                }

                decimal giaGio;

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT GiaTheoGio FROM Phong WHERE SoPhong=@SoPhong",
                    conn, tran))
                {
                    cmd.Parameters.AddWithValue("@SoPhong", soPhong);
                    giaGio = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                DateTime ngayTra = DateTime.Now;
                double soGio = Math.Max(
                    1, Math.Ceiling((ngayTra - ngayNhan).TotalHours));

                decimal tienPhong = (decimal)soGio * giaGio;
                decimal tienDichVu;

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT ISNULL(SUM(s.SoLuong*d.DonGia),0)
                      FROM SuDungDichVu s
                      INNER JOIN DichVu d ON s.MaDV=d.MaDV
                      WHERE s.MaPhieu=@MaPhieu",
                    conn, tran))
                {
                    cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                    tienDichVu = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                decimal tongTien = tienPhong + tienDichVu;

                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE PhieuDatPhong
                      SET NgayTraPhong=@NgayTra,
                          TongTienThanhToan=@TongTien
                      WHERE MaPhieu=@MaPhieu AND NgayTraPhong IS NULL",
                    conn, tran))
                {
                    cmd.Parameters.AddWithValue("@NgayTra", ngayTra);
                    cmd.Parameters.AddWithValue("@TongTien", tongTien);
                    cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);

                    if (cmd.ExecuteNonQuery() != 1)
                        throw new Exception("Phiếu đã được thanh toán.");
                }

                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Phong SET TrangThai=N'Đang dọn dẹp' WHERE SoPhong=@SoPhong",
                    conn, tran))
                {
                    cmd.Parameters.AddWithValue("@SoPhong", soPhong);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return tongTien;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}