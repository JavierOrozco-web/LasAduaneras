using System.ComponentModel.DataAnnotations;

public class Cliente
{
    [Key]
    public string ClienteID { get; set; }
    
    public string NombreCompleto { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public string Correo { get; set; }
}