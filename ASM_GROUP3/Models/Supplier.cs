using System;
using System.Collections.Generic;

namespace ASM_GROUP3.Models
{
    public partial class Supplier
    {
        public int SupplierId { get; set; }                   // Khóa chính
        public string? SupplierName { get; set; }             // Tên nhà cung cấp
        public string? Phone { get; set; }                    // Số điện thoại
        public string? Address { get; set; }                  // Địa chỉ

        public string? SupplierCode { get; set; }             // Mã nhà cung cấp
        public string? ContactPerson { get; set; }            // Người liên hệ chính
        public bool? IsActive { get; set; } = true;           // Còn hoạt động (nullable)
        public DateTime? CooperationStartDate { get; set; }   // Ngày hợp tác (nullable)

        public virtual ICollection<ImportOrder> ImportOrders { get; set; } = new List<ImportOrder>();
    }
}
