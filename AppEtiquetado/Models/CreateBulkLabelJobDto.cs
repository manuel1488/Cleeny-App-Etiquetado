namespace AppEtiquetado.Models;

public class CreateBulkLabelJobDto
{
    public long ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitMeasureCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int LabelCount { get; set; } = 1;
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
}
