using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.IoC;
using Stellantis.ProjectName.WebApi;
using Stellantis.ProjectName.WebApi.Extensions;
using Stellantis.ProjectName.WebApi.Filters;
using System.Globalization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ValidatorOptions.Global.LanguageManager.Enabled = false;
ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en-US");

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

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

#if DEBUG

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

#else
builder.Services.AddDbContext<Context>(options => options.UseSqlServer(configuration["ConnectionString"]));
#endif

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
            .AllowAnyHeader();
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
app.UseAuthorization();
app.MapControllers();
app.UsePathBase("/");
await app.RunAsync().ConfigureAwait(false);