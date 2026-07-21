using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Application.Options;
using CaioMatheusDev.Api.Application.Services;
using CaioMatheusDev.Api.Infrastructure.Auth;
using CaioMatheusDev.Api.Infrastructure.Email;
using CaioMatheusDev.Api.Infrastructure.GitHub;
using CaioMatheusDev.Api.Infrastructure.Http;
using CaioMatheusDev.Api.Infrastructure.Persistence;
using CaioMatheusDev.Api.Infrastructure.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var defaultConnection = NormalizePostgresConnectionString(
    builder.Configuration.GetConnectionString("DefaultConnection"));
var authSection = builder.Configuration.GetSection("AuthLab");
var authOptions = authSection.Get<AuthLabOptions>() ?? new AuthLabOptions();

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
builder.Services.Configure<AuthLabOptions>(authSection);
builder.Services.Configure<AuthFlowOptions>(authSection);
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection("Resend"));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = authOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Name
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var error = context.AuthenticateFailure is SecurityTokenExpiredException
                    ? new ApiError("expired_token", "JWT expired. Login again.")
                    : new ApiError("missing_or_invalid_token", "Send a valid token using the Authorization: Bearer header.");

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(error));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(
                    new ApiError("forbidden", "You do not have permission to access this resource.")));
            }
        };
    });
builder.Services.AddAuthorization();

if (!string.IsNullOrWhiteSpace(defaultConnection))
{
    builder.Services.AddDbContext<PortfolioDbContext>(options =>
        options.UseNpgsql(defaultConnection));
    builder.Services.AddScoped<IAuthUserStore, PostgresAuthUserStore>();
    builder.Services.AddScoped<IPasswordResetTokenStore, PostgresPasswordResetTokenStore>();
    builder.Services.AddScoped<IBackendRoomStore, PostgresBackendRoomStore>();
    builder.Services.AddScoped<IWalletStore, PostgresWalletStore>();
}
else
{
    builder.Services.AddSingleton<IAuthUserStore, InMemoryAuthUserStore>();
    builder.Services.AddSingleton<IPasswordResetTokenStore, InMemoryPasswordResetTokenStore>();
    builder.Services.AddSingleton<IBackendRoomStore, InMemoryBackendRoomStore>();
    builder.Services.AddSingleton<IWalletStore, InMemoryWalletStore>();
}

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

builder.Services.AddHttpClient<ResendEmailSender>(client =>
{
    client.BaseAddress = new Uri("https://api.resend.com/");
    client.Timeout = TimeSpan.FromSeconds(12);
});

builder.Services.AddSingleton<IPortfolioService, PortfolioService>();
builder.Services.AddSingleton<IGitHubService, GitHubService>();
builder.Services.AddSingleton<IStatusCheckService, StatusCheckService>();
builder.Services.AddSingleton<IAddressLookupService, AddressLookupService>();
builder.Services.AddSingleton<IIntegrationLabService, IntegrationLabService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<SmtpEmailSender>();
builder.Services.AddScoped<IEmailSender>(serviceProvider =>
{
    var resendOptions = serviceProvider.GetRequiredService<IOptions<ResendOptions>>().Value;

    return string.IsNullOrWhiteSpace(resendOptions.ApiKey)
        ? serviceProvider.GetRequiredService<SmtpEmailSender>()
        : serviceProvider.GetRequiredService<ResendEmailSender>();
});
builder.Services.AddScoped<IAuthLabService, AuthLabService>();
builder.Services.AddScoped<IBackendRoomService, BackendRoomService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddHostedService<PortfolioRefreshWorker>();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (!string.IsNullOrWhiteSpace(defaultConnection))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Redirect("/openapi/v1.json"));
app.MapControllers();

app.Run();

static string? NormalizePostgresConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return null;
    }

    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
        (uri.Scheme is not "postgres" and not "postgresql"))
    {
        return connectionString;
    }

    var credentials = uri.UserInfo.Split(':', 2);
    var username = credentials.Length > 0 ? Uri.UnescapeDataString(credentials[0]) : string.Empty;
    var password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');
    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
    var sslMode = query["sslmode"] ?? query["sslMode"] ?? "Require";
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = database,
        Username = username,
        Password = password,
        SslMode = Enum.TryParse<SslMode>(sslMode, true, out var parsedSslMode)
            ? parsedSslMode
            : SslMode.Require
    };

    return builder.ConnectionString;
}
