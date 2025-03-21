using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.IoC;
using Stellantis.ProjectName.WebApi;
using Stellantis.ProjectName.WebApi.Extensions;
using Stellantis.ProjectName.WebApi.Filters;
using Stellantis.ProjectName.Application.Validators;
using System.Globalization;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.WebApi.Dto.Validators;
using Stellantis.ProjectName.WebApi.Dto;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ValidatorOptions.Global.LanguageManager.Enabled = false;
ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en-US");

var configuration = new ConfigurationBuilder()
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
var databaseType = configuration["DatabaseType"];
switch (databaseType)
{
    case "InMemory":
        builder.Services.AddDbContext<Context>(options => options.UseInMemoryDatabase(databaseName: "InMemory")
        );
        break;
    case "SqlServer":
        builder.Services.AddDbContext<Context>(options => options.UseSqlServer(configuration["ConnectionString"]));
        break;
    default:
        throw new NotSupportedException(databaseType);
}

builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ValidateMemberDtoFilter>(); 


builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var arrLanguage = new[] { "en-US", "pt-BR", "es-AR", "fr-FR", "it-IT", "nl-NL" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = arrLanguage;
    options
        .SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

#if DEBUG
builder.Services.AddAuthentication("AuthenticationForDebug")
    .AddScheme<AuthenticationSchemeOptions, ForDebugAuthenticationHandler>("AuthenticationForDebug", null);
#endif

var app = builder.Build();
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UsePathBase("/");
await app.RunAsync().ConfigureAwait(false);
