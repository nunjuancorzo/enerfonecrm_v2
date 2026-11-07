namespace EnerfoneCRM.Models;

public sealed record ConsultaVolti(string Modulo, string Operacion, string Estado, string Tipo,
    string Busqueda, string FechaDesde, string FechaHasta, int Limite, string Orden);

public sealed record RegistroVolti(int Id, string Nombre, string Estado, string Tipo, DateTime? Fecha,
    string Comercial = "", decimal? Comision = null);

public sealed record ReferenciaVolti(string Etiqueta, string Ruta);
public sealed record RespuestaVolti(string Texto, IReadOnlyList<ReferenciaVolti> Referencias);