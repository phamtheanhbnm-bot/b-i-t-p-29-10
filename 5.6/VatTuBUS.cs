
using System;
using System.Data;
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class VatTuBUS
    {
        private readonly VatTuDAL dal = new VatTuDAL();

        public DataTable GetAll() => dal.GetAll();

        public DataTable Search(string keyword)
            => dal.Search(keyword ?? "");

        public DataTable GetLowStock() => dal.GetLowStock();

        public decimal GetInventoryValue() => dal.GetInventoryValue();

        public void Insert(VatTuDTO v)
        {
            if (string.IsNullOrWhiteSpace(v.MaVatTu) ||
                string.IsNullOrWhiteSpace(v.TenVatTu))
                throw new Exception("Mã và tên vật tư không được để trống.");

            if (v.SoLuongTon < 0 || v.MucTonAnToan < 0)
                throw new Exception("Số lượng tồn và mức an toàn không được âm.");

            dal.Insert(v);
        }

        public void Update(VatTuDTO v)
        {
            if (string.IsNullOrWhiteSpace(v.MaVatTu) ||
                string.IsNullOrWhiteSpace(v.TenVatTu))
                throw new Exception("Mã và tên vật tư không được để trống.");

            if (v.MucTonAnToan < 0)
                throw new Exception("Mức tồn an toàn không được âm.");

            dal.Update(v);
        }

        public void Delete(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                throw new Exception("Hãy chọn vật tư cần xóa.");

            dal.Delete(ma);
        }
    }
}
