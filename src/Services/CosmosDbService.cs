namespace todo
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using todo.Models;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Fluent;
    using Microsoft.Extensions.Configuration;

    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }
        
        public async Task AddItemAsync(Item item)
        {
            await this._container.CreateItemAsync<Item>(item, new PartitionKey(item.Id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await this._container.DeleteItemAsync<SensorDataItemID>(id, new PartitionKey(id));


        }

        public async Task<SensorDataItemID> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<SensorDataItemID> response = await this._container.ReadItemAsync<SensorDataItemID>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { 
                return null;
            }

        }

        public async Task<IEnumerable<SensorDataItemID>> GetItemsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<SensorDataItemID>(new QueryDefinition(queryString));
            List<SensorDataItemID> results = new List<SensorDataItemID>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateItemAsync(string id, SensorDataItemID item)
        {
            await this._container.UpsertItemAsync<SensorDataItemID>(item, new PartitionKey(id));
        }
    }
}
