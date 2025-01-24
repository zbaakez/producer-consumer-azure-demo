using DatabaseService;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<DataHelper>();

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

app.MapPost("store-data", async (Data data, DataHelper dataHelper) =>
{
    try
    {
        await dataHelper.StoreData(data);
    }
    catch (Exception e)
    {
        Console.WriteLine("Error storing data: " + e.Message);
        return Results.BadRequest(e.Message);
    }
    return Results.Ok();
});

app.MapGet("get-data", async (DataHelper dataHelper) =>
{
    try 
    {
        List<Data> data = await dataHelper.GetDataAsync(10);
        if (data.Count == 0)
        {
            return Results.NoContent();
        }
        return Results.Ok(data);
    }
    catch (Exception e)
    {
        Console.WriteLine("Error getting data: " + e.Message);
        return Results.BadRequest(e.Message);
    }
});

app.Run();
