
using InventoryManagement.DAL;
using InventoryManagement.DTO;

namespace InventoryManagement.BUS
{
    public class PhieuKhoBUS
    {
        private readonly PhieuKhoDAL dal = new PhieuKhoDAL();

        public int Save(PhieuKhoDTO p)
        {
            return dal.Save(p);
        }
    }
}
