namespace AppEtiquetado.Models;

public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public IList<T> Items { get; set; } = [];
}
