using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMvc(opt =>
{
    // set to true to disable buffering, meaning that the request/repsonse body is read as a stream synchronously
    opt.SuppressInputFormatterBuffering = false;
    opt.SuppressOutputFormatterBuffering = false;
}).AddNewtonsoftJson(opt =>
{
    // if the request/response body is larger than 8kb, it will be buffered into temporary files
    opt.InputFormatterMemoryBufferThreshold = 8 * 1024; //8kb
    opt.OutputFormatterMemoryBufferThreshold = 8 * 1024; //8kb
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// cities stored in memory
var cities = new List<City>
{
    new("Beijing", "China", ["Great Wall", "Forbidden City", "Temple of Heaven"]),
    new("Xi'an", "China", ["Terracotta Army", "City Wall", "Big Wild Goose Pagoda"]),
    new("New York", "USA", ["Statue of Liberty", "Central Park", "Empire State Building"]),
    new("Paris", "France", ["Eiffel Tower", "Louvre Museum", "Notre-Dame Cathedral"]),
    new("London", "UK", ["Big Ben", "Tower Bridge", "Buckingham Palace"]),
    new("Tokyo", "Japan", ["Tokyo Tower", "Senso-ji Temple", "Shibuya Crossing"]),
};

app.MapGet("/cities", () =>
    {
        return Results.Ok(cities.Select(c => new
        {
            City = c,
            Weathers = Enumerable.Range(1, 7).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        Random.Shared.Next(1, 99),
                        Random.Shared.Next(1, 20)
                    ))
                .ToArray()
        }));
    })
    .WithName("GetCities")
    .WithOpenApi();

(List<City> valid, List<City> invalid) ValidateInputCities(City[] newCities)
{
    return newCities.Aggregate((valid: new List<City>(), invalid: new List<City>()), (acc, val) =>
    {
        if (acc.valid.Any(c => c.Name == val.Name) || // duplicated in request
            cities.Any(c => c.Name == val.Name)) // duplicated in store
        {
            acc.invalid.Add(val);
        }
        else
        {
            acc.valid.Add(val);
        }

        return acc;
    });
}

app.MapPost("/cities",
        ([FromBody] City[] newCities) =>
        {
            var (_, invalid) = ValidateInputCities(newCities);
            if (invalid.Count > 0)
            {
                return Results.BadRequest(
                    $"Cities '{string.Join(", ", invalid.Select(c => c.Name))}' already exist");
            }

            newCities.AsParallel().ForAll(nc => cities.Add(nc));

            return Results.Created();
        })
    .WithName("PostCities")
    .WithOpenApi();

app.Run();

record City(string Name, string Country, string[] Showplaces);

record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    int Humidity,
    int Visibility
)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}