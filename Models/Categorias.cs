using System.ComponentModel.DataAnnotations;

public class Categorias
{
    [Key]
    public int CategoriaID { get; set; }

    public string NombreCategoria { get; set; }
}