using LibraryRecommendation.Api.Data;
using LibraryRecommendation.Api.Repositories;
using LibraryRecommendation.Api.Services;
using LibraryRecommendation.Api.Services.Recommendations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// The TF-IDF model is expensive to build, so it is cached for the lifetime of the app and
// rebuilt only when the book set changes (BookRepository calls Invalidate()).
builder.Services.AddSingleton<IBookVectorCache, BookVectorCache>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

var app = builder.Build();

// Apply migrations and seed the demo library on startup (no-op once data exists).
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await DbSeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
