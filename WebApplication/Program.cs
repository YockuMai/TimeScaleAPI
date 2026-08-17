using Microsoft.EntityFrameworkCore;
using WebApplication.Application.Interfaces;
using WebApplication.Application.Services;
using WebApplication.Model;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICsvParser, CsvParser>();
builder.Services.AddScoped<IAggregationCalculator, AggregationCalculator>();
builder.Services.AddScoped<IAggregationFilter, AggregationFilter>();
builder.Services.AddScoped<IValueFilter, ValueFilter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
