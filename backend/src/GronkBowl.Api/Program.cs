using System.Text.Json.Serialization;
using GronkBowl.Api;
using GronkBowl.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddDbContext<GronkBowlDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

const string FrontendDevCorsPolicy = "FrontendDev";
builder.Services.AddCors(options => options.AddPolicy(FrontendDevCorsPolicy, policy =>
    policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GronkBowlDbContext>();
    db.Database.Migrate();
    DbSeeder.SeedIfEmpty(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendDevCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
