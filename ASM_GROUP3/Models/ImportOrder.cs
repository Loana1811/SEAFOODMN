using ASM_GROUP3.Models;

public partial class ImportOrder
{
    public int OrderId { get; set; }
    public int? SupplierId { get; set; }
    public DateTime? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }

    public virtual ICollection<ImportOrderDetail> ImportOrderDetails { get; set; } = new List<ImportOrderDetail>();
    public virtual Supplier Supplier { get; set; }
}
