using Newtonsoft.Json;

namespace Orders.Core.Models;

public class ItemsDataModel
{
    [JsonProperty("items")]
    public Item[] Items { get; set; }
}