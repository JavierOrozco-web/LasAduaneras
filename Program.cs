using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Agregar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔥 CORS (para tu HTML)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 🔥 activar CORS
app.UseCors("AllowAll");


// 🔥 Endpoint REAL desde BD
app.MapGet("/productos", async (AppDbContext db) =>
{
    return await db.Productos.ToListAsync();
});

app.MapGet("/categorias", async (AppDbContext db) =>
{
    return await db.Categorias.ToListAsync();
});

app.Run();