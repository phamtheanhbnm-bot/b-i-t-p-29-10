
using System;
using System.Data;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class NhaCungCapBUS
    {
        private readonly NhaCungCapDAL dal = new NhaCungCapDAL();

        public DataTable GetAll() => dal.GetAll();

        public void Insert(NhaCungCapDTO n)
        {
            if (string.IsNullOrWhiteSpace(n.TenNCC))
                throw new Exception("Tên nhà cung cấp không được để trống.");

            dal.Insert(n);
        }

        public void Update(NhaCungCapDTO n)
        {
            if (n.MaNCC <= 0 || string.IsNullOrWhiteSpace(n.TenNCC))
                throw new Exception("Thông tin nhà cung cấp không hợp lệ.");

            dal.Update(n);
        }

        public void Delete(int ma)
        {
            if (ma <= 0)
                throw new Exception("Hãy chọn nhà cung cấp cần xóa.");

            dal.Delete(ma);
        }
    }
}
