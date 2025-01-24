using System.Collections.Concurrent;
using System.Text;
using Producer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Produces new data and sends it to the Database Service.
app.MapGet("produce", async () =>
{
    Console.WriteLine("Producing data...");
    var tasks = new List<Task<bool>>();
    int successfulTasksCount = 0;
    
    for (int i = 0; i < 10; i++)
    {
        var task = Task.Run(async () =>
        {
            Data data = new Data();
            bool success = await SendToDatabaseServiceAsync(data);
            if (success)
            {
                Interlocked.Increment(ref successfulTasksCount);
            }
            return success;
        });
        tasks.Add(task);
    }

    var timeoutTask = Task.Delay(2000);
    var completedTask = await Task.WhenAny(Task.WhenAll(tasks), timeoutTask);

    int successfulCount = completedTask == timeoutTask
        ? successfulTasksCount
        : tasks.Count(t => t.IsCompletedSuccessfully && t.Result);

    return "{\"producedCount\": " + successfulCount + "}";
});

async Task<bool> SendToDatabaseServiceAsync(Data data)
{
    using (var client = new HttpClient())
    {
        var json = data.ToJson();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(
            "https://database-service-app.internal.ambitiousmoss-94b84a66.westeurope.azurecontainerapps.io/store-data",
            content);
        return response.IsSuccessStatusCode;
    }
}


app.Run();