using System;
using System.Collections.Generic;

namespace ASM_GROUP3.Models;

public partial class Order
{
    public int OrderId { get; set; }
    public bool IsPrinted { get; set; } = false;

    public DateTime? CreatedDate { get; set; }

    public string? Customer { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public string? Phone { get; set; }          // ➕ Mới thêm
    public string? Address { get; set; }        // ➕ Mới thêm
    public string? Note { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
