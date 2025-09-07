using System.Data;
using System.ComponentModel.DataAnnotations;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;


var builder = WebApplication.CreateBuilder(args);

// (1) IConfiguration demo
var agencyName = builder.Configuration["Agency:Name"] ?? "Unknown Agency";

// Register SQLite/Dapper
builder.Services.AddScoped<IDbConnection>(_ => new SqliteConnection("Data Source=intel.db"));

var app = builder.Build();

app.UseExceptionHandler("/errors");  // (12)
app.UseStaticFiles();                // (2)

// ---------- DB setup ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();
    db.Open();
    DbSeeder.EnsureTables(db);
    DbSeeder.SeedIfEmpty(db);
}

// ---------- Error Handler (13) ----------
app.Map("/errors", (HttpContext ctx) =>
{
    var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
    var ex = feature?.Error;
    var status = ex is AccessViolationException ? 403 : 500;
    return Results.Problem(
        title: ex is AccessViolationException ? "Access denied" : "Unexpected error",
        detail: ex?.Message,
        statusCode: status,
        instance: feature?.Path
    );
});

// ---------- Endpoints ----------

// (1) IConfiguration
app.MapGet("/agency/info", () => Results.Ok(new { Agency = agencyName }));

// (4) Endpoint group
var people = app.MapGroup("/people");

// Simple validator
var validate = (object dto) =>
{
    var ctx = new ValidationContext(dto);
    Validator.ValidateObject(dto, ctx, validateAllProperties: true);
};

// (3,5,6,7) Create person
// (3,5,6,7) Create person
people.MapPost("/", async Task<Results<Created<Person>, ValidationProblem>>
    (IDbConnection db, PersonCreateDto dto) =>
{
    // Capability 3: validate using DataAnnotations
    var ctx = new ValidationContext(dto);
    var results = new List<ValidationResult>();

    if (!Validator.TryValidateObject(dto, ctx, results, true))
    {
        return TypedResults.ValidationProblem(
            results.ToDictionary(
                r => r.MemberNames.FirstOrDefault() ?? "General",
                r => new[] { r.ErrorMessage ?? "Invalid" })
        );
    }

    // Insert valid record
    var id = await db.ExecuteScalarAsync<long>(
        "INSERT INTO People (FullName,Category,Affiliation,AccessLevel) " +
        "VALUES (@FullName,@Category,@Affiliation,@AccessLevel); " +
        "SELECT last_insert_rowid();",
        dto);

    // ✅ Explicit column list so Dapper matches Person record constructor
    var person = await db.QuerySingleAsync<Person>(
        "SELECT Id, FullName, Category, Affiliation, AccessLevel, Alias, RealPersonWiki " +
        "FROM People WHERE Id=@Id", new { Id = id });

    return TypedResults.Created($"/people/{id}", person);
})
.WithName("CreatePerson");





// (5,7,10,13) Get person by ID
people.MapGet("/{id:int}", async Task<IResult> (int id, IDbConnection db, HttpContext ctx) =>
{
    var person = await db.QuerySingleOrDefaultAsync<Person>("SELECT * FROM People WHERE Id=@Id", new { Id = id });
    if (person is null) return TypedResults.NotFound();

    var lvlHeader = ctx.Request.Headers["X-Access-Level"].FirstOrDefault();
    if (!int.TryParse(lvlHeader, out var callerLevel)) callerLevel = 1;

    if (callerLevel < person.AccessLevel)
        throw new AccessViolationException($"Need {person.AccessLevel}, got {callerLevel}");

    return TypedResults.Ok(person);
})
.WithName("GetPersonById");

// (8,9) Search people with LinkGenerator + query
people.MapGet("/search", async (string q, LinkGenerator links, HttpContext ctx, IDbConnection db) =>
{
    var results = await db.QueryAsync<Person>("SELECT * FROM People WHERE FullName LIKE @P", new { P = $"%{q}%" });
    return Results.Ok(results.Select(p => new
    {
        Person = p,
        Link = links.GetPathByName(ctx, "GetPersonById", new { id = p.Id })
    }));
});

// (15) Similar but not matching route
people.MapGet("/by-alias/{alias}", (string alias) =>
{
    return Results.Ok(new { Alias = alias, Note = "Stub alias search" });
});

// (14) Multiple paths handled by same endpoint
async Task<IResult> GetLocation(int id, IDbConnection db)
{
    var loc = await db.QuerySingleOrDefaultAsync<Location>("SELECT * FROM Locations WHERE Id=@Id", new { Id = id });
    return loc is null ? Results.NotFound() : Results.Ok(loc);
}
app.MapGet("/loc/{id:int}", GetLocation);
app.MapGet("/places/{id:int}", GetLocation);

// (11) Endpoint filter demo
var protocols = app.MapGroup("/protocols").AddEndpointFilter(async (ctx, next) =>
{
    Console.WriteLine($"[protocols] {DateTime.UtcNow:o} {ctx.HttpContext.Request.Path}");
    return await next(ctx);
});

// Protocols list by location (10 header binding too)
protocols.MapGet("/by-location/{locationId:int}", async (int locationId, IDbConnection db, HttpContext ctx) =>
{
    var agent = ctx.Request.Headers["X-Agent-Id"].FirstOrDefault() ?? "anonymous";
    Console.WriteLine($"Viewed protocols for {locationId} by {agent}");
    var rows = await db.QueryAsync<Protocol>("SELECT * FROM Protocols WHERE LocationId=@Id", new { Id = locationId });
    return Results.Ok(rows);
});

// ---------- Default root ----------
app.MapGet("/", () => Results.Text("OK. Try /agency/info, /people, /people/search?q=alija, /loc/1, /protocols/by-location/1"));

app.Run();

// ---------- Records ----------
public record Person(
    int Id,
    string FullName,
    string Category,
    string Affiliation,
    int AccessLevel,
    string? Alias,
    string? RealPersonWiki
);

public record PersonCreateDto(
    [Required] string FullName,
    [MaxLength(50)] string Category,
    [Required] string Affiliation,
    [Range(1, 9)] int AccessLevel
);

public record Location(int Id, string? Name, string City, string Country, string Currency);
public record Protocol(int Id, int LocationId, string Title, string ConciseGuideline);
