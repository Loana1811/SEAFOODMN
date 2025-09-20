using ASM_GROUP3.Models;

public partial class Seafood
{
    public int SeafoodId { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public decimal? Quantity { get; set; }  // ✅ sửa từ int? → decimal?
    public string? Unit { get; set; }

    public decimal? UnitPrice { get; set; }
    public DateTime? ImportDate { get; set; }
    public bool? IsAvailable { get; set; }
    public virtual ICollection<ImportOrderDetail> ImportOrderDetails { get; set; } = new List<ImportOrderDetail>();
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    // ✅ THÊM THUỘC TÍNH NÀY ĐỂ HIỂN THỊ COMBOBOX
    public string DisplayInfo => $"{Name} - {(UnitPrice.HasValue ? UnitPrice.Value.ToString("N0") : "0")} đ";
}
