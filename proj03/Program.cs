using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// (1) IConfiguration demo

var agencyName = builder.Configuration["Agency:Name"] ?? "Unknown Agency";
var agencies = builder.Configuration.GetSection("Agencies").Get<List<Agency>>() ?? new();



var app = builder.Build();

// (2) Static files hosting
app.UseStaticFiles();

// (11/12) Exception handler
app.UseExceptionHandler("/errors");

// ---------- Error Handler (12/13) ----------
app.Map("/errors", (HttpContext ctx) =>
{
    var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
    var ex = feature?.Error;
    var status = 500;
    return Results.Problem(
        title: "Unexpected error",
        detail: ex?.Message,
        statusCode: status,
        instance: feature?.Path
    );
});

// ---------- In-memory data ----------
var placesStore = new List<Place>();

try
{
    var placesJson = File.ReadAllText("places.json");
    var places = JsonSerializer.Deserialize<List<Place>>(placesJson, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (places is not null)
    {
        placesStore.AddRange(places);
        Console.WriteLine($"Loaded {placesStore.Count} places from JSON");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading places JSON: {ex.Message}");
}




var peopleStore = new List<Person>();

try
{
    var json = File.ReadAllText("people.json");
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    var people1 = JsonSerializer.Deserialize<List<Person>>(json, options);

    if (people1 is not null)
    {
        peopleStore.AddRange(people1);
        Console.WriteLine($"Loaded {peopleStore.Count} people from JSON");
    }
    else
    {
        Console.WriteLine("JSON file was empty or not parsed correctly.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading JSON: {ex.Message}");
}

// (4) Endpoint group
var people = app.MapGroup("/people");

// Manual validation helper
void Validate(object dto)
{
    var ctx = new ValidationContext(dto);
    Validator.ValidateObject(dto, ctx, validateAllProperties: true);
}

// (3,5,6,7) Create person with validation
people.MapPost("/", async Task<IResult> (HttpContext ctx) =>
{
    // Deserialize body into DTO
    var dto = await ctx.Request.ReadFromJsonAsync<PersonCreateDto>();
    if (dto is null)
    {
        return TypedResults.BadRequest(new { error = "Invalid JSON body" });
    }

    // Manual validation
    try
    {
        Validate(dto);
    }
    catch (ValidationException vex)
    {
        return TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["ValidationError"] = new[] { vex.Message }
        });
    }

// Create new person
var newPerson = new Person(
    Id: peopleStore.Count + 1,
    Name: dto.FullName,
    Category: dto.Category,
    Affiliation: dto.Affiliation,
    AccessLevel: dto.AccessLevel.ToString(),
    Alias: null,
    RealPersonWiki: null,
    LocationId: (peopleStore.Count * 41)
    );

    peopleStore.Add(newPerson);

    return TypedResults.Created($"/people/{newPerson.Id}", newPerson);
})
.WithName("CreatePerson");  // (7) .WithName()

// GET all people
people.MapGet("/", (HttpContext ctx) =>
{
    return TypedResults.Ok(peopleStore);
});



// (5,7,10,13) Get person by ID


// (8,9) Search with query + LinkGenerator
people.MapGet("/search", (string q, LinkGenerator links, HttpContext ctx) =>
{
    var results = peopleStore
        .Where(p =>
            typeof(Person).GetProperties()
                .Where(prop => prop.PropertyType == typeof(string))
                .Select(prop => prop.GetValue(p) as string)
                .Any(val => val?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
        )
        .Select(p => new
        {
            Person = p,
            Link = links.GetPathByName(ctx, "GetPersonById", new { id = p.Id })
        });

    return Results.Ok(results);
});


// (9,15) Get person by alias
people.MapGet("/by-alias/{alias}", (string alias) =>
{
    var person = peopleStore
        .FirstOrDefault(p => string.Equals(p.Alias, alias, StringComparison.OrdinalIgnoreCase));

    return person is null
        ? Results.NotFound(new { Message = $"No person found with alias '{alias}'" })
        : Results.Ok(person);
});

// Capability 12 - Force an error so .UseExceptionHandler() can catch it
app.MapGet("/force-error", () =>
{
    throw new Exception("This is a forced test error");
});



// (11) Endpoint filter demo
var protocols = app.MapGroup("/protocols").AddEndpointFilter(async (ctx, next) =>
{
    Console.WriteLine($"[protocols] {DateTime.UtcNow:o} {ctx.HttpContext.Request.Path}");
    return await next(ctx);
});

// Protocols stub (just to demonstrate)
protocols.MapGet("/by-location/{id:int}", (int id, HttpContext ctx) =>
{
    var agent = ctx.Request.Headers["X-Agent-Id"].FirstOrDefault() ?? "anonymous";
    return Results.Ok(new { LocationId = id, ViewedBy = agent });
});

// (14) Multiple paths handled by same endpoint
IResult GetLocation(int id)
{
    var place = placesStore.FirstOrDefault(p => p.Id == id);
    return place is null
        ? Results.NotFound(new { Message = $"No place found with Id {id}" })
        : Results.Ok(place);
}

app.MapGet("/loc/{id:int}", GetLocation);
app.MapGet("/places/{id:int}", GetLocation);


// ---------- Root ----------
app.MapGet("/", () => Results.Text("OK. Try /agency/info, /people, /people/search?q=test"));

app.MapGet("/agencies", () => Results.Ok(agencies));

app.MapGet("/agency/info", () => Results.Ok(new { Agency = agencyName }));

app.Run();

// ---------- Records ----------
public record Person(
    int Id,

    [property: JsonPropertyName("name")]
    string Name,

    [property: JsonPropertyName("category")]
    string Category,

    [property: JsonPropertyName("affiliation")]
    string Affiliation,

    [property: JsonPropertyName("accessLevel")]
    string AccessLevel,  // string, since JSON has "Secret", "Top Secret"

    [property: JsonPropertyName("alias")]
    string? Alias,

    [property: JsonPropertyName("realPersonWiki")]
    string? RealPersonWiki,

    [property: JsonPropertyName("locationId")]
    int? LocationId
);

public record PersonCreateDto(
    [Required] string FullName,
    [MaxLength(50)] string Category,
    [Required] string Affiliation,
    [Range(1, 9)] int AccessLevel
);
public record Agency(int Id, string Name, string Country);

public record Place(
    int Id,
    string? Name,
    string City,
    string Country,
    string Currency
);
