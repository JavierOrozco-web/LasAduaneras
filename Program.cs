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

app.MapGet("/productos", async (int? categoriaID, AppDbContext db) =>
{
    var query = db.Productos.AsQueryable();

    // 1. Aplicamos el filtro de categoría si existe (opcional en la URL)
    if (categoriaID != null)
    {
        query = query.Where(p => p.CategoriaID == categoriaID);
    }

    // 2. Seleccionamos SOLO lo necesario para que el JSON sea ligero
    var resultado = await query
        .Select(p => new 
        {
            p.ProductoID,
            p.NombreProducto,
            p.Precio
        })
        .ToListAsync();

    return resultado;
});

app.MapPost("/login", async (LoginRequest req, AppDbContext db) =>
{
    // 🔍 Buscar en CLIENTES
    var cliente = await db.Clientes
        .FirstOrDefaultAsync(u => 
            u.Correo == req.Correo && 
            u.Password == req.Password);

    if (cliente != null)
    {
        return Results.Ok(new
        {
            tipo = "cliente",
            id = cliente.ClienteID,
            nombre = cliente.NombreCompleto
        });
    }

    // 🔍 Buscar en EMPLEADOS
    var empleado = await db.Empleados
        .FirstOrDefaultAsync(u => 
            u.Correo == req.Correo && 
            u.Password == req.Password);

    if (empleado != null)
    {
        return Results.Ok(new
        {
            tipo = "empleado",
            id = empleado.EmpleadoID,
            nombre = empleado.Nombre
        });
    }

    // ❌ No encontrado
    return Results.Unauthorized();
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
