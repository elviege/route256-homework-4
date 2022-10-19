using Newtonsoft.Json;

namespace Orders.Core.Models;

public class Item
{
    [JsonProperty("item_id")]
    public long ItemId { get; set; }
    [JsonProperty("count")]
    public int Count { get; set; }
}