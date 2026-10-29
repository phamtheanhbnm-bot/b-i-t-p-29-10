
using System.Data;
using System.Data.SqlClient;
using InventoryManagement.DTO;

namespace InventoryManagement.DAL
{
    public class NhaCungCapDAL
    {
        public DataTable GetAll()
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlDataAdapter da = new SqlDataAdapter(
                "SELECT * FROM NhaCungCap ORDER BY MaNCC", conn))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public int Insert(NhaCungCapDTO n)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"INSERT INTO NhaCungCap(TenNCC, DienThoai, DiaChi)
                  VALUES(@ten, @dt, @dc)", conn))
            {
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 150).Value = n.TenNCC;
                cmd.Parameters.Add("@dt", SqlDbType.VarChar, 15).Value =
                    string.IsNullOrWhiteSpace(n.DienThoai)
                    ? (object)System.DBNull.Value : n.DienThoai;
                cmd.Parameters.Add("@dc", SqlDbType.NVarChar, 255).Value =
                    (object)n.DiaChi ?? System.DBNull.Value;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(NhaCungCapDTO n)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                @"UPDATE NhaCungCap SET TenNCC=@ten,
                  DienThoai=@dt, DiaChi=@dc WHERE MaNCC=@ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = n.MaNCC;
                cmd.Parameters.Add("@ten", SqlDbType.NVarChar, 150).Value = n.TenNCC;
                cmd.Parameters.Add("@dt", SqlDbType.VarChar, 15).Value =
                    string.IsNullOrWhiteSpace(n.DienThoai)
                    ? (object)System.DBNull.Value : n.DienThoai;
                cmd.Parameters.Add("@dc", SqlDbType.NVarChar, 255).Value =
                    (object)n.DiaChi ?? System.DBNull.Value;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(int ma)
        {
            using (SqlConnection conn = Database.GetConnection())
            using (SqlCommand cmd = new SqlCommand(
                "DELETE FROM NhaCungCap WHERE MaNCC=@ma", conn))
            {
                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = ma;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }
    }
}
