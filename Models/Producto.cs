using System.ComponentModel.DataAnnotations;

public class Producto
{
    [Key]
    public int ProductoID { get; set; }
    
    public string NombreProducto { get; set; }
    public decimal Precio { get; set; }
    public string ProveedorSugerido { get; set; }
    public string CategoriaID { get; set; }
}