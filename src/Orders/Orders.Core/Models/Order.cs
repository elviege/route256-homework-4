using Newtonsoft.Json.Linq;

namespace Orders.Core.Models;

public class Order
{
    public Guid? Id { get; set; }
    public Client Client { get; set; }
    public DateTime? CreationDt { get; set; }
    public DateTime? IssueDt { get; set; }
    public Status Status { get; set; }
    public Item[] Items { get; set; }
    //public string ItemsData { get; set; }
    public Warehouse Warehouse { get; set; }
}