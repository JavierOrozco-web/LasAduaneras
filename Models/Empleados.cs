using System.ComponentModel.DataAnnotations;

public class Empleados
{
    [Key]
    public string EmpleadoID { get; set; }

    public string NombreCompleto { get; set; }
    public string Puesto { get; set; }
    public string Correo { get; set; }
    public string Contrasena { get; set; }
}