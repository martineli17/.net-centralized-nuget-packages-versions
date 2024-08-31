using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapPost("/serialize", ([FromBody] Payload payload) =>
{
    var result = JsonConvert.SerializeObject(payload);
    return Results.Ok(result);
})
.WithName("SerializeObject")
.WithOpenApi();

app.Run();

public readonly struct Payload
{
    public string Name { get; init; }
}