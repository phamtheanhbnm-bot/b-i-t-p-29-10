namespace HotelManagement
{
    public class ProductDTO
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; } = "";
        public decimal DonGia { get; set; }
    }

    public class ServiceDTO
    {
        public int MaPhieu { get; set; }
        public int MaDV { get; set; }
        public int SoLuong { get; set; }
        public DateTime ThoiGian { get; set; }
    }

    public class PhongDTO
    {
        public string SoPhong { get; set; } = "";
        public string LoaiPhong { get; set; } = "";
        public decimal GiaTheoGio { get; set; }
        public string TrangThai { get; set; } = "";
    }

    public class KhachHangDTO
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; } = "";
        public string CCCD { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
    }
}