using Microsoft.EntityFrameworkCore;
using WebAppTest.Data;
using WebAppTest.Models;

var builder = WebApplication.CreateBuilder(args);

// Use SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();

var app = builder.Build();
// In Program.cs after builder.Build()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Name = "Laptop", Price = 1200 },
            new Product { Name = "Mouse", Price = 25 }
        );
        db.SaveChanges();
    }
}

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();

// CRUD endpoints for Product

// CREATE
app.MapPost("/api/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return product;
});

// READ ALL
app.MapGet("/api/products", async (AppDbContext db) =>
    await db.Products.ToListAsync());

// READ ONE
app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is Product p ? Results.Ok(p) : Results.NotFound());

// UPDATE
app.MapPut("/api/products/{id}", async (int id, Product input, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product == null) return Results.NotFound();

    product.Name = input.Name;
    product.Price = input.Price;

    await db.SaveChangesAsync();
    return Results.Ok(product);
});

// DELETE
app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product != null)
    {
        db.Products.Remove(product);
        await db.SaveChangesAsync();
    }
    return Results.Ok();
});

// Map Razor Pages
app.MapRazorPages();

app.Run();
