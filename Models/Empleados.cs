using System.ComponentModel.DataAnnotations;

public class Empleados
{
    [Key]
    public string EmpleadoID { get; set; }

    public string Nombre { get; set; }
    public string Apellidos { get; set; }
    public string Puesto { get; set; }
    public string Correo { get; set; }
    public string Contrasena { get; set; }
}