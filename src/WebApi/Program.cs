using System.Globalization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.IoC;
using Stellantis.ProjectName.WebApi;
using Stellantis.ProjectName.WebApi.Extensions;
using Stellantis.ProjectName.WebApi.Filters;
using Stellantis.ProjectName.WebApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Stellantis.ProjectName.WebApi.Configurations; 


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ValidatorOptions.Global.LanguageManager.Enabled = false;
ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en-US");

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var signingConfigurations = new SigningConfigurations(Environment.GetEnvironmentVariable("SECRET_KEY"));
builder.Services.AddSingleton(signingConfigurations);

var tokenConfigurations = new TokenConfigurations();
new ConfigureFromConfigurationOptions<TokenConfigurations>(
    configuration.GetSection("TokenConfigurations"))
    .Configure(tokenConfigurations);

builder.Services.AddSingleton(tokenConfigurations);

builder.Services.AddAuthentication(authOptions =>
{
    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(bearerOptions =>
{
    var paramsValidation = bearerOptions.TokenValidationParameters;

    bearerOptions.RequireHttpsMetadata = false;
    bearerOptions.SaveToken = true;

    paramsValidation.ValidateIssuer = false;
    paramsValidation.ValidateAudience = false;
    paramsValidation.IssuerSigningKey = signingConfigurations.Key;
    paramsValidation.ValidAudience = tokenConfigurations.Audience;
    paramsValidation.ValidIssuer = tokenConfigurations.Issuer;

    paramsValidation.ValidateIssuerSigningKey = true;
    paramsValidation.ValidateLifetime = true;
    paramsValidation.ClockSkew = TimeSpan.Zero;
});

builder.Services.AddControllers();
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<ValidateModelStateFilterAttribute>();
#if !DEBUG
    opt.Filters.Add(typeof(CustomExceptionFilter));
#endif
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMvcCore();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.Configure<ApiBehaviorOptions>(p =>
{
    p.SuppressModelStateInvalidFilter = true;
});
builder.Services.ConfigureDependencyInjection();
builder.Services.RegisterMapper();

builder.Services.AddSignalR();
builder.Services.AddScoped<Func<ISmtpClient>>(sp => () => sp.GetRequiredService<ISmtpClient>());
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISmtpClient, SmtpClientWrapper>();

string? databaseType = configuration["DatabaseType"];
switch (databaseType)
{
    case "InMemory":
        builder.Services.AddDbContext<Context>(options => options.UseInMemoryDatabase(databaseName: "InMemory"));
        break;
    case "SqlServer":
        builder.Services.AddDbContext<Context>(options => options.UseSqlServer(configuration["ConnectionString"]));
        break;
    default:
        throw new NotSupportedException(databaseType);
}

string[] arrLanguage = ["en-US", "pt-BR", "es-AR", "fr-FR", "it-IT", "nl-NL"];
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    string[] supportedCultures = arrLanguage;
    options
        .SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    options.RequestCultureProviders =
    [
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

#if DEBUG

builder.Services.AddLocalization();
builder.Services.AddAuthentication("AuthenticationForDebug")
    .AddScheme<AuthenticationSchemeOptions, ForDebugAuthenticationHandler>("AuthenticationForDebug", null);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins("http://localhost:3000")
            .AllowCredentials();
    });
});

#endif

WebApplication app = builder.Build();

#if DEBUG

await ForDebugHelper.LoadDataForTestsAsync(databaseType, app).ConfigureAwait(false);

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#endif

RequestLocalizationOptions localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chat");
app.MapHub<NotificationHub>("/notification");
app.UsePathBase("/");
await app.RunAsync().ConfigureAwait(false);