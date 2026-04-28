using System.ComponentModel.DataAnnotations;

public class Cliente
{
    [Key]
    public string ClienteID { get; set; }
    
    public string Nombre { get; set; }
    public string Apellidos { get; set;}
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public string Correo { get; set; }
    public string Contrasena { get; set; }
}