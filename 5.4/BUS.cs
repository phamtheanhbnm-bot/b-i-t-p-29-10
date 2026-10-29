using System.Data;
using Microsoft.Data.SqlClient;

namespace HotelManagement
{
    public class BUS
    {
        private readonly DAL dal = new DAL();

        public DataTable GetRooms()
        {
            return dal.GetData(
                "SELECT SoPhong,LoaiPhong,GiaTheoGio,TrangThai FROM Phong");
        }

        public DataTable GetAvailableRooms()
        {
            return dal.GetData(
                "SELECT SoPhong,LoaiPhong,GiaTheoGio,TrangThai FROM Phong WHERE TrangThai=N'Trống'");
        }

        public DataTable GetServices()
        {
            return dal.GetData(
                "SELECT MaDV,TenDV,DonGia FROM DichVu");
        }

        public DataTable GetActiveBookings()
        {
            return dal.GetData(
                @"SELECT p.MaPhieu,p.SoPhong,k.HoTen,k.CCCD,p.NgayNhanPhong
                  FROM PhieuDatPhong p
                  INNER JOIN KhachHang k ON p.MaKH=k.MaKH
                  WHERE p.NgayTraPhong IS NULL");
        }

        public DataTable GetBookingDetails(int maPhieu)
        {
            return dal.GetData(
                @"SELECT d.TenDV,s.SoLuong,d.DonGia,
                         s.SoLuong*d.DonGia AS ThanhTien,s.ThoiGian
                  FROM SuDungDichVu s
                  INNER JOIN DichVu d ON s.MaDV=d.MaDV
                  WHERE s.MaPhieu=@MaPhieu
                  ORDER BY s.ThoiGian DESC",
                new SqlParameter("@MaPhieu", maPhieu));
        }

        public DataTable SearchCustomer(string cccd)
        {
            return dal.GetData(
                @"SELECT MaKH,HoTen,CCCD,SoDienThoai
                  FROM KhachHang WHERE CCCD=@CCCD",
                new SqlParameter("@CCCD", cccd));
        }

        public int CheckIn(
            string hoTen, string cccd, string sdt, string soPhong)
        {
            if (string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(cccd) ||
                string.IsNullOrWhiteSpace(soPhong))
                throw new Exception("Vui lòng nhập đầy đủ thông tin.");

            return dal.CheckIn(hoTen.Trim(), cccd.Trim(),
                sdt.Trim(), soPhong);
        }

        public void AddService(int maPhieu, int maDV, int soLuong)
        {
            if (maPhieu <= 0 || maDV <= 0 || soLuong <= 0)
                throw new Exception("Thông tin dịch vụ không hợp lệ.");

            dal.AddService(maPhieu, maDV, soLuong);
        }

        public decimal CheckOut(int maPhieu)
        {
            if (maPhieu <= 0)
                throw new Exception("Vui lòng chọn phiếu nhận phòng.");

            return dal.CheckOut(maPhieu);
        }
    }
}