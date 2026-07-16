using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Application.Services;
using CaioMatheusDev.Api.Infrastructure.GitHub;
using CaioMatheusDev.Api.Infrastructure.Http;
using CaioMatheusDev.Api.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true));
});

builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("github", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("caio-matheus-dev-portfolio");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
});

builder.Services.AddHttpClient("default", client =>
{
    client.Timeout = TimeSpan.FromSeconds(6);
});

builder.Services.AddSingleton<IPortfolioService, PortfolioService>();
builder.Services.AddSingleton<IGitHubService, GitHubService>();
builder.Services.AddSingleton<IStatusCheckService, StatusCheckService>();
builder.Services.AddSingleton<IAddressLookupService, AddressLookupService>();
builder.Services.AddSingleton<IIntegrationLabService, IntegrationLabService>();
builder.Services.AddHostedService<PortfolioRefreshWorker>();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Redirect("/openapi/v1.json"));
app.MapControllers();

app.Run();
