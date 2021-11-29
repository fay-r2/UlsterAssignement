namespace todo
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using todo.Models;

    public interface ICosmosDbService
    {
        Task<IEnumerable<SensorDataItemID>> GetItemsAsync(string query);
        Task<SensorDataItemID> GetItemAsync(string id);
        Task AddItemAsync(Item item);
        Task UpdateItemAsync(string id, SensorDataItemID item);
        Task DeleteItemAsync(string id);
    }
}
