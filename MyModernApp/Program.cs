using Microsoft.EntityFrameworkCore;
using MyModernApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add API Controllers
builder.Services.AddControllers();

// Configure Entity Framework with SQL Server or SQLite
// Use environment variable or config to determine provider
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite", false) 
    || builder.Environment.IsDevelopment() && connectionString!.StartsWith("Data Source=");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
    {
        // SQLite connection - for development/testing
        options.UseSqlite(connectionString);
    }
    else
    {
        // SQL Server connection - for production
        options.UseSqlServer(
            connectionString,
            sqlOptions =>
            {
                // Enable retry on transient failures
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                
                // Set command timeout to prevent long-running queries
                sqlOptions.CommandTimeout(30);
            });
    }

    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Modern App API", 
        Version = "v1",
        Description = "API for managing orders"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Modern App API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
