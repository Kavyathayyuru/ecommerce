using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUi", policy =>
    {
        var allowedOrigin = builder.Configuration["Frontend:AllowedOrigin"] ?? "http://localhost:5173";
        policy.WithOrigins(allowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("WebUi");

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "ECommerce Microservices Gateway";
        options.SwaggerEndpoint("/api/product/swagger/v1/swagger.json", "Product Service");
        options.SwaggerEndpoint("/api/order/swagger/v1/swagger.json", "Order Service");
        options.SwaggerEndpoint("/api/user/swagger/v1/swagger.json", "User Service");
        options.SwaggerEndpoint("/api/payment/swagger/v1/swagger.json", "Payment Service");
    });
}

app.MapReverseProxy();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
