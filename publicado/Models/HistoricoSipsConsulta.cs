using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("historico_sips_consultas")]
public class HistoricoSipsConsulta
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("cups")]
    [StringLength(32)]
    public string Cups { get; set; } = string.Empty;

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("usuario_nombre")]
    [StringLength(100)]
    public string? UsuarioNombre { get; set; }

    [Column("usuario_email")]
    [StringLength(255)]
    public string? UsuarioEmail { get; set; }

    [Column("fecha_consulta")]
    public DateTime FechaConsulta { get; set; }

    [Column("success")]
    public bool Success { get; set; } = true;

    [Column("http_status_code")]
    public int? HttpStatusCode { get; set; }

    [Column("error_message")]
    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    [Column("response_json")]
    public string? ResponseJson { get; set; }

    [Column("response_size")]
    public int? ResponseSize { get; set; }
}
