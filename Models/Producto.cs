using System.ComponentModel.DataAnnotations;

public class Producto
{
    [Key]
    public string ProductoID { get; set; }
    
    public string NombreProducto { get; set; }
    public decimal Precio { get; set; }
    public string ProveedorSugerido { get; set; }
    public int CategoriaID { get; set; }
}
