
using System;
using System.Collections.Generic;

namespace InventoryManagement.DTO
{
    public class PhieuKhoDTO
    {
        public string LoaiPhieu { get; set; }
        public DateTime NgayLap { get; set; }
        public int? MaNCC { get; set; }
        public List<ChiTietPhieuDTO> ChiTiet { get; set; }
            = new List<ChiTietPhieuDTO>();
    }
}
