using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models;

[Table("optime_solicitudes")]
public class OptimeSolicitud
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("razon_social")]
    [Required(ErrorMessage = "La razón social es obligatoria")]
    [StringLength(200)]
    public string RazonSocial { get; set; } = string.Empty;

    [Column("cif_nif")]
    [Required(ErrorMessage = "El CIF/NIF del PdV es obligatorio")]
    [StringLength(20)]
    public string CifNif { get; set; } = string.Empty;

    [Column("direccion_fiscal")]
    [Required(ErrorMessage = "La dirección fiscal es obligatoria")]
    [StringLength(250)]
    public string DireccionFiscal { get; set; } = string.Empty;

    [Column("poblacion")]
    [Required(ErrorMessage = "La población es obligatoria")]
    [StringLength(100)]
    public string Poblacion { get; set; } = string.Empty;

    [Column("provincia")]
    [Required(ErrorMessage = "La provincia es obligatoria")]
    [StringLength(100)]
    public string Provincia { get; set; } = string.Empty;

    [Column("codigo_postal")]
    [Required(ErrorMessage = "El código postal es obligatorio")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 dígitos")]
    public string CodigoPostal { get; set; } = string.Empty;

    [Column("persona_contacto")]
    [Required(ErrorMessage = "La persona de contacto es obligatoria")]
    [StringLength(150)]
    public string PersonaContacto { get; set; } = string.Empty;

    [Column("nif_persona_contacto")]
    [Required(ErrorMessage = "El NIF de la persona de contacto es obligatorio")]
    [StringLength(20)]
    public string NifPersonaContacto { get; set; } = string.Empty;

    [Column("telefono")]
    [Required(ErrorMessage = "El teléfono de contacto es obligatorio")]
    [RegularExpression(@"^\+?[0-9 ]{9,20}$", ErrorMessage = "El teléfono no es válido")]
    public string Telefono { get; set; } = string.Empty;

    [Column("email")]
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Column("cnae")]
    [RegularExpression(@"^\d{1,10}$", ErrorMessage = "El CNAE debe ser numérico, sin letras ni espacios")]
    public string? Cnae { get; set; }

    [Column("iban")]
    [RegularExpression(@"^ES\d{2}$", ErrorMessage = "El IBAN debe tener el formato ES00")]
    public string? Iban { get; set; }

    [NotMapped]
    public string? IbanDigitos
    {
        get => Iban != null && Iban.StartsWith("ES") ? Iban[2..] : Iban;
        set => Iban = string.IsNullOrWhiteSpace(value) ? null : "ES" + value.Trim();
    }

    [Column("ccc")]
    [RegularExpression(@"^\d{20}$", ErrorMessage = "El Nº CCC debe tener 20 dígitos")]
    public string? Ccc { get; set; }

    [Column("swift")]
    [RegularExpression(@"^[A-Za-z0-9]{8}([A-Za-z0-9]{3})?$", ErrorMessage = "El SWIFT debe tener 8 u 11 caracteres")]
    public string? Swift { get; set; }

    [Column("titular_cuenta")]
    [StringLength(200)]
    public string? TitularCuenta { get; set; }

    [Column("interes_lowi")]
    public bool InteresLowi { get; set; }

    [Column("interes_vodafone")]
    public bool InteresVodafone { get; set; }

    [Column("interes_rentik")]
    public bool InteresRentik { get; set; }

    [Column("interes_3d_seguridad")]
    public bool Interes3DSeguridad { get; set; }

    [Column("acepta_codigo_buenas_practicas")]
    public bool AceptaCodigoBuenasPracticas { get; set; }

    [Column("acepta_adhesion_pdv")]
    public bool AceptaAdhesionPdv { get; set; }

    [Column("fecha_firma")]
    public DateOnly FechaFirma { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("fecha_modificacion")]
    public DateTime? FechaModificacion { get; set; }

    [NotMapped]
    public string IbanCompleto => string.IsNullOrWhiteSpace(Ccc) ? string.Empty : $"{Iban}{Ccc}";
}

[Table("optime_documentos")]
public class OptimeDocumento
{
    public const string TipoCopiaCif = "Copia CIF";
    public const string TipoCopiaEscrituras = "Copia Escrituras";
    public const string TipoCopiaDni = "Copia DNI Representante";
    public const string TipoFirma = "Firma";

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("solicitud_id")]
    public int SolicitudId { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty;

    [Column("nombre_archivo")]
    [StringLength(255)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Column("content_type")]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Column("contenido")]
    public byte[] Contenido { get; set; } = Array.Empty<byte>();

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
