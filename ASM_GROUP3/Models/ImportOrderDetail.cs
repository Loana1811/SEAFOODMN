using ASM_GROUP3.Models;

public partial class ImportOrderDetail
{
    public int DetailId { get; set; }
    public int OrderId { get; set; }  // Đây là khóa ngoại tham chiếu tới ImportOrder
    public int SeafoodId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal Subtotal => Quantity * UnitPrice;

    public virtual ImportOrder Order { get; set; }  // Thuộc tính điều hướng tới ImportOrder
    public virtual Seafood Seafood { get; set; }
}
