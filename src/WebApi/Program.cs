using CleanArchBase.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using IoC;
using System.Globalization;
using WebApi.Extensions;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ValidatorOptions.Global.LanguageManager.Enabled = false;
ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("fr");

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

builder.Services.AddControllers();
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(typeof(ValidateModelStateFilterAttribute));
    opt.Filters.Add(typeof(CustomException));
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
builder.Services.AddDbContext<CleanArchBaseContext>(options => options.UseSqlServer(configuration["ConnectionString"]));
builder.Services.AddLocalization();

var arrLanguage = new[] { "en-US", "pt-BR" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = arrLanguage;
    options.SetDefaultCulture(supportedCultures[1])
             .AddSupportedCultures(supportedCultures)
             .AddSupportedUICultures(supportedCultures);
});

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

app.UsePathBase("/api");

app.Run();
