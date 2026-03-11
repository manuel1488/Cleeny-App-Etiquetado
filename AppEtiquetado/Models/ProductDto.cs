namespace AppEtiquetado.Models;

public class ProductDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Brand { get; set; } = string.Empty;
    public int UnitMeasureId { get; set; }
    public string UnitMeasureName { get; set; } = string.Empty;
    public string UnitMeasureCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsPartialSaleAllowed { get; set; }
    public bool AllowCustomPricing { get; set; }
    public bool IsActive { get; set; }

    public override string ToString() => $"{Code} - {Name}";
}
