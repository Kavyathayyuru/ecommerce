using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHttpClient();

var signingKey = builder.Configuration["Authentication:SigningKey"]
    ?? throw new InvalidOperationException("Authentication:SigningKey must be configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:Audience"],
            ValidateLifetime = true,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUi", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173", "http://127.0.0.1:5173"];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("WebUi");
app.UseAuthentication();
app.UseAuthorization();

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

app.MapGet("/api/admin/services", async (
    IHttpClientFactory clientFactory,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var services = new Dictionary<string, string>
    {
        ["ProductService"] = configuration["Monitoring:ProductServiceUrl"] ?? string.Empty,
        ["OrderService"] = configuration["Monitoring:OrderServiceUrl"] ?? string.Empty,
        ["UserService"] = configuration["Monitoring:UserServiceUrl"] ?? string.Empty,
        ["PaymentService"] = configuration["Monitoring:PaymentServiceUrl"] ?? string.Empty
    };
    var client = clientFactory.CreateClient();
    var results = await Task.WhenAll(services.Select(async service =>
    {
        if (string.IsNullOrWhiteSpace(service.Value))
        {
            return new { name = service.Key, status = "not-configured", url = service.Value };
        }

        try
        {
            using var response = await client.GetAsync($"{service.Value.TrimEnd('/')}/health", cancellationToken);
            return new { name = service.Key, status = response.IsSuccessStatusCode ? "healthy" : "unhealthy", url = service.Value };
        }
        catch (HttpRequestException)
        {
            return new { name = service.Key, status = "unavailable", url = service.Value };
        }
    }));
    return Results.Ok(results);
}).RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
