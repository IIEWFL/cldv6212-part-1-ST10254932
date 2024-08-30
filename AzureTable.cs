using Azure;
using Azure.Data.Tables;
using System;
using System.Threading.Tasks;

namespace cldvPart1
{
    public class azuretb
    {
        private AzureTableStorage _azureTableStorage;

        public azuretb(string storageConnectionString, string tableName)
        {
            _azureTableStorage = new AzureTableStorage(storageConnectionString, tableName);
        }

        public async Task InsertOrMergeCustomerEntityAsync(CustomerProfile entity)
        {
            await _azureTableStorage.UpsertEntityAsync(entity);
        }

        public async Task DeleteCustomerEntityAsync(string partitionKey, string rowKey)
        {
            await _azureTableStorage.DeleteEntityAsync(partitionKey, rowKey);
        }
    }

    public class AzureTableStorage
    {
        private TableClient _tableClient;

        public AzureTableStorage(string storageConnectionString, string tableName)
        {
            var serviceClient = new TableServiceClient(storageConnectionString);
            _tableClient = serviceClient.GetTableClient(tableName);
            _tableClient.CreateIfNotExists();
        }

        public async Task UpsertEntityAsync(CustomerProfile entity)
        {
            await _tableClient.UpsertEntityAsync(entity);
        }

        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }

    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "ConsumerProfile";
        public string RowKey { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}