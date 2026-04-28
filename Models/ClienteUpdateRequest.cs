namespace LasAduaneras.Models
{
    public class ClienteUpdateRequest
    {
        public string ClienteID { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Correo { get; set; }
        public string Password { get; set; }
    }
}