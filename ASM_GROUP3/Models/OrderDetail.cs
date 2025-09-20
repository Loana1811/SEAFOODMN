using ASM_GROUP3.Models;

public class OrderDetail
{
    public int OrderDetailId { get; set; }
    public int? OrderId { get; set; }
    public int? SeafoodId { get; set; }

    public decimal? Quantity { get; set; }    // ✅ dùng double để hỗ trợ số lẻ
    public decimal? UnitPrice { get; set; }
    public decimal? Subtotal { get; set; }

    public virtual Order? Order { get; set; }
    public virtual Seafood? Seafood { get; set; }
}
