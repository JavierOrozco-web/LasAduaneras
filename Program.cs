    using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// 🧪 TEST (sin base de datos)
app.MapGet("/test", () => "Funciona");

// Endpoints
app.MapGet("/productos", async (int? categoriaID, AppDbContext db) =>
{
    var query = db.Productos.AsQueryable();

    if (categoriaID != null)
        query = query.Where(p => p.CategoriaID == categoriaID);

    return await query.ToListAsync();
});

app.MapGet("/productos", async (int? categoriaID, AppDbContext db) =>
{
    var query = db.Productos.AsQueryable();

    // 1. Aplicamos el filtro de categoría si existe
    if (categoriaID != null)
    {
        query = query.Where(p => p.CategoriaID == categoriaID);
    }

    // 2. Seleccionamos SOLO lo necesario (C# crea el JSON con estos nombres)
    var resultado = await query
        .Select(p => new 
        {
            p.NombreProducto, // Asegúrate que se llame así en tu clase C#
            p.Precio
        })
        .ToListAsync();

    return resultado;
});

app.MapGet("/categorias", async (AppDbContext db) =>
{
    return await db.Categorias.ToListAsync();
});

app.MapGet("/clientes", async (AppDbContext db) =>
{
    return await db.Clientes.ToListAsync();
});

app.MapGet("/error", () =>
{
    return Results.Problem("Ocurrió un error en el servidor");
});

app.Run();
