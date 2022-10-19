namespace Orders.Core.Models;

public class OrderInsertModel
{
    public Guid Id { get; set; }
    public long ClientId { get; set; }
    public DateTime CreationDt { get; set; }
    public DateTime? IssueDt { get; set; }
    public int StatusId { get; set; }
    //public Item[] Items { get; set; }
    public string ItemsData { get; set; }
    public long WarehouseId { get; set; }
}