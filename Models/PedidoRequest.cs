using System.Collections.Generic;

namespace LasAduaneras.Models
{
    public class PedidoRequest
    {
        public string ClienteID { get; set; }
        public string MetodoPago { get; set; }
        public decimal Total { get; set; }

        public List<ProductoRequest> Productos { get; set; }
    }

    public class ProductoRequest
    {
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}