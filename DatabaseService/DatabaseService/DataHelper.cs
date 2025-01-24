using Microsoft.Azure.Cosmos;

namespace DatabaseService;

public class DataHelper
{
    
    private readonly string _endpointUri;
    private readonly string _primaryKey;
    private readonly string _databaseId;
    private readonly string _containerId;
        
    private CosmosClient? _cosmosClient;
    private Database? _database;
    private Container? _container;
    
    
    public DataHelper(IConfiguration configuration)
    {
        _endpointUri = configuration["CosmosDb:EndpointUri"] ?? throw new ArgumentNullException("CosmosDb:EndpointUri");
        _primaryKey = configuration["CosmosDb:PrimaryKey"] ?? throw new ArgumentNullException("CosmosDb:PrimaryKey");
        _databaseId = configuration["CosmosDb:DatabaseId"] ?? throw new ArgumentNullException("CosmosDb:DatabaseId");
        _containerId = configuration["CosmosDb:ContainerId"] ?? throw new ArgumentNullException("CosmosDb:ContainerId");
        Console.WriteLine(configuration["CosmosDb:EndpointUri"]);
        SetupConnectionAsync().Wait();
        Console.WriteLine("DataHelper created");
    }
       

    public async Task StoreData(Data data)
    {
        if (!await VerifyConnectionAsync())
        {
            throw new Exception("Connection to database not established");
        }
        try
        {
            await _container.CreateItemAsync(data, new PartitionKey(data.id));
            Console.WriteLine("Data stored");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error storing data: " + e.Message);
            throw;
        }
        
    }
    
    public async Task<List<Data>> GetDataAsync(int count)
    {
        if (!await VerifyConnectionAsync())
        {
            throw new Exception("Connection to database not established");
        }
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.Consumed = false ORDER BY c._ts ASC OFFSET 0 LIMIT @count")
                .WithParameter("@count", count);
            using var it = _container.GetItemQueryIterator<Data>(query);
            var results = new List<Data>();
            while (it.HasMoreResults)
            {
                var response = await it.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            Console.WriteLine("Data retrieved. Count: " + results.Count);
            // Mark data as consumed
            foreach (var item in results)
            {
                item.Consumed = true;
                await _container.ReplaceItemAsync(item, item.id, new PartitionKey(item.id));
            }
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error getting data: (probably no data available) " + e.Message);
            return new List<Data>();
        }
    }
    
    private async Task<bool> SetupConnectionAsync()
    {
        try 
        {
            _cosmosClient = new CosmosClient(_endpointUri, _primaryKey);
            _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);
            _container = await _database.CreateContainerIfNotExistsAsync(_containerId, "/id");
            Console.WriteLine("Connection to database established");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error setting up connection: " + e.Message);
            return false;
        }
    }
    
    private async Task<bool> VerifyConnectionAsync()
    {
        if (_cosmosClient == null || _database == null || _container == null)
        {
            return await SetupConnectionAsync();
        }
        return true;
    }

}