using System.ComponentModel.DataAnnotations;
public class DetallePedido
{
    [Key]
    public int DetalleID { get; set; }

    public int PedidoID { get; set; }

    public string ProductoID { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }
}
