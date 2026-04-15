using System.ComponentModel.DataAnnotations;

namespace EnerfoneCRM.Models;

public class LoginModel
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RecordarMe { get; set; }
}
