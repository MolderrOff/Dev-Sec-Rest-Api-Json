using DevSecApi.Application.Services;
using Microsoft.AspNetCore.Builder;
using DevSecApi.Domain.Repositories;
using DevSecApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
    })
    ;
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<PayloadService>();
builder.Services.AddScoped<IPayloadRepository, PayloadRepository>();

var app = builder.Build();


if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevSec API V1");
        c.RoutePrefix = "api/swagger";

    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
