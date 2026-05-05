using Zapas.Api.Services;
using Zapas.Api.Middleware;
using Zapas.Api.Data;
using Zapas.Api.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

builder.Services.AddDbContext<ZapasDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("ZapasDb"));
});

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
