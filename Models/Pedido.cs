using System.ComponentModel.DataAnnotations;
public class Pedido
{
    [Key]
    public int PedidoID { get; set; }

    public string ClienteID { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public string MetodoPago { get; set; }
}
