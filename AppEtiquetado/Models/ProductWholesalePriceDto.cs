namespace AppEtiquetado.Models;

public class ProductWholesalePriceDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public int WholesaleTierId { get; set; }
    public string TierName { get; set; } = string.Empty;
    public decimal MinQuantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal? FixedPrice { get; set; }
    public bool IsActive { get; set; }
}
