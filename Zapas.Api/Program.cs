using Zapas.Api.Services;
using Zapas.Api.Middleware;
using Zapas.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();
