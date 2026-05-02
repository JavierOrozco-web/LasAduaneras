using Microsoft.EntityFrameworkCore;
using LasAduaneras.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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

app.MapGet("/test", () => "Funciona");

app.MapGet("/cliente/{id}", async (string id, AppDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(id);
    return cliente is not null ? Results.Ok(cliente) : Results.NotFound();
});

app.MapPut("/usuario/editar", async (ClienteUpdateRequest req, AppDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(req.ClienteID);

    if (cliente != null)
    {
        cliente.Nombre = req.Nombre;
        cliente.Apellidos = req.Apellidos;
        cliente.Telefono = req.Telefono;
        cliente.Direccion = req.Direccion;
        cliente.Correo = req.Correo;

        if (!string.IsNullOrEmpty(req.Password))
            cliente.Contrasena = req.Password;

        await db.SaveChangesAsync();
        return Results.Ok(new { mensaje = "Cliente actualizado" });
    }

    var empleado = await db.Empleados.FindAsync(req.ClienteID);

    if (empleado != null)
    {
        empleado.Nombre = req.Nombre;
        empleado.Apellidos = req.Apellidos;
        empleado.Correo = req.Correo;

        if (!string.IsNullOrEmpty(req.Password))
            empleado.Contrasena = req.Password;

        await db.SaveChangesAsync();
        return Results.Ok(new { mensaje = "Empleado actualizado" });
    }

    return Results.NotFound("Usuario no encontrado");
});

app.MapPost("/verificar-password", async (VerificarPasswordRequest req, AppDbContext db) =>
{
    var usuario = await db.Clientes
        .FirstOrDefaultAsync(u => u.ClienteID == req.Id && u.Contrasena == req.Password);

    if (usuario == null)
    {
        var empleado = await db.Empleados
            .FirstOrDefaultAsync(u => u.EmpleadoID == req.Id && u.Contrasena == req.Password);

        if (empleado == null)
            return Results.Unauthorized();
    }

    return Results.Ok();
});

app.MapGet("/empleado/{id}", async (string id, AppDbContext db) =>
{
    var empleado = await db.Empleados.FindAsync(id);
    return empleado is not null ? Results.Ok(empleado) : Results.NotFound();
});

app.MapGet("/productos", async (int? categoriaID, AppDbContext db) =>
{
    var query = db.Productos.AsQueryable();

    if (categoriaID != null)
    {
        query = query.Where(p => p.CategoriaID == categoriaID);
    }

    var resultado = await query
        .Select(p => new 
        {
            p.ProductoID,
            p.NombreProducto,
            p.Precio,
            p.Stock
        })
        .ToListAsync();

    return resultado;
});

app.MapGet("/productos/buscar", async (string nombre, AppDbContext db) =>
{
    var productos = await db.Productos
        .Where(p => p.NombreProducto.Contains(nombre))
        .Select(p => new {
            p.ProductoID,
            p.NombreProducto,
            p.Precio
        })
        .ToListAsync();

    return Results.Ok(productos);
});

app.MapPost("/login", async (LoginRequest req, AppDbContext db) =>
{
    var cliente = await db.Clientes
        .FirstOrDefaultAsync(u => 
            u.Correo == req.Correo && 
            u.Contrasena == req.Password);

    if (cliente != null)
    {
        return Results.Ok(new {
            tipo = "cliente",
            id = cliente.ClienteID,
            nombre = cliente.Nombre,
            apellidos = cliente.Apellidos,
            telefono = cliente.Telefono,
            direccion = cliente.Direccion,
            correo = cliente.Correo,
            contrasena = cliente.Contrasena
        });
    }

    var empleado = await db.Empleados
        .FirstOrDefaultAsync(u => 
            u.Correo == req.Correo && 
            u.Contrasena == req.Password);

    if (empleado != null)
    {
        return Results.Ok(new
        {
            tipo = "empleado",
            id = empleado.EmpleadoID,
            nombre = empleado.Nombre,
            apellidos = empleado.Apellidos,
            puesto = empleado.Puesto,
            correo = empleado.Correo,
            contrasena = empleado.Contrasena
        });
    }

    return Results.Unauthorized();
});

app.MapPost("/registro", async (RegistroRequest req, AppDbContext db) =>
{
    var existe = await db.Clientes.AnyAsync(c => c.Correo == req.Correo);
    if (existe)
        return Results.BadRequest("El correo ya está registrado");

    var ultimo = await db.Clientes
        .OrderByDescending(c => c.ClienteID)
        .Select(c => c.ClienteID)
        .FirstOrDefaultAsync();

    int nuevoNumero = 1;

    if (ultimo != null)
    {
        var numero = int.Parse(ultimo.Split('-')[1]);
        nuevoNumero = numero + 1;
    }

    string nuevoID = $"CLI-{nuevoNumero.ToString("D3")}";

    var cliente = new Cliente
    {
        ClienteID = nuevoID,
        Nombre = req.Nombre,
        Apellidos = req.Apellidos,
        Correo = req.Correo,
        Contrasena = req.Password,
        Telefono = req.Telefono,
        Direccion = req.Direccion
    };

    db.Clientes.Add(cliente);
    await db.SaveChangesAsync();

    return Results.Ok(new { id = nuevoID });
});

app.MapPost("/pedido", async (PedidoRequest req, AppDbContext db) =>
{
    var pedido = new Pedido
    {
        ClienteID = req.ClienteID,
        Fecha = DateTime.UtcNow,
        Total = req.Total,
        MetodoPago = req.MetodoPago
    };

    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync();

    foreach (var item in req.Productos)
    {
        db.DetallePedidos.Add(new DetallePedido
        {
            PedidoID = pedido.PedidoID,
            ProductoID = item.ProductoID,
            Cantidad = item.Cantidad,
            Precio = item.Precio
        });
    }

    await db.SaveChangesAsync();

    return Results.Ok(new { mensaje = "Pedido creado" });
});

app.MapGet("/historial/{clienteID}", async (string clienteID, AppDbContext db) =>
{
    var pedidos = await db.Pedidos
        .Where(p => p.ClienteID == clienteID)
        .Select(p => new
        {
            p.PedidoID,
            p.Fecha,
            p.Total,
            p.MetodoPago,

            Productos = db.DetallePedidos
                .Where(d => d.PedidoID == p.PedidoID)
                .Join(db.Productos,
                      d => d.ProductoID,
                      pr => pr.ProductoID,
                      (d, pr) => new
                      {
                          d.ProductoID,
                          d.Cantidad,
                          d.Precio,
                          pr.NombreProducto
                      })
                .ToList()
        })
        .OrderByDescending(p => p.Fecha)
        .ToListAsync();

    return Results.Ok(pedidos);
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
