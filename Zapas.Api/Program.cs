using Zapas.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddZapasOptions(builder.Configuration)
    .AddZapasApplicationServices()
    .AddZapasSecurity(builder.Configuration, builder.Environment)
    .AddZapasPersistence(builder.Configuration)
    .AddZapasApiDocumentation();

var app = builder.Build();

app.UseZapasPipeline();

app.Run();

public partial class Program;
