using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnerfoneCRM.Models
{
    [Table("clientes_simple")]
    public class Cliente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("tipo_cliente")]
        [Required(ErrorMessage = "El tipo de cliente es obligatorio")]
        public string? TipoCliente { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(255)]
        public string? Nombre { get; set; }

        [Column("dni_cif")]
        [MaxLength(50)]
        public string? DniCif { get; set; }

        [Column("dni_representante")]
        [MaxLength(50)]
        public string? Dni { get; set; }

        [Column("email")]
        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Email no válido")]
        public string? Email { get; set; }

        [Column("telefono")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Column("tipo_via")]
        [MaxLength(50)]
        public string? TipoVia { get; set; }

        [Column("direccion")]
        [MaxLength(500)]
        public string? Direccion { get; set; }

        [Column("numero")]
        [MaxLength(20)]
        public string? Numero { get; set; }

        [Column("escalera")]
        [MaxLength(10)]
        public string? Escalera { get; set; }

        [Column("piso")]
        [MaxLength(10)]
        public string? Piso { get; set; }

        [Column("puerta")]
        [MaxLength(10)]
        public string? Puerta { get; set; }

        [Column("aclarador")]
        [MaxLength(255)]
        public string? Aclarador { get; set; }

        [Column("poblacion")]
        [MaxLength(100)]
        public string? Poblacion { get; set; }

        [Column("provincia")]
        [MaxLength(100)]
        public string? Provincia { get; set; }

        [Column("codigo_postal")]
        [MaxLength(10)]
        public string? CodigoPostal { get; set; }

        [Column("iban")]
        [MaxLength(34)]
        public string? Iban { get; set; }

        [Column("representante")]
        [MaxLength(255)]
        public string? Representante { get; set; }

        [Column("comercial")]
        [MaxLength(255)]
        public string? Comercial { get; set; }

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("fecha_alta")]
        public DateTime? FechaAlta { get; set; }

        [Column("copia_recibo_bancario")]
        [MaxLength(500)]
        public string? CopiaReciboBancario { get; set; }

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }

        [NotMapped]
        public string? NombreUsuario { get; set; }
    }
}
