using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services
{
    public class ContratoService
    {
        private readonly DbContextProvider _dbContextProvider;

        public ContratoService(DbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<List<Contrato>> ObtenerTodosPorTipoAsync(string tipo, string? rolUsuario = null, string? nombreUsuario = null, int? usuarioId = null)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            // Todos los campos VARCHAR/TEXT envueltos en COALESCE para evitar NULL casting
            var sql = @"
                SELECT 
                    id, 
                    COALESCE(tipo, '') as tipo,
                    COALESCE(estado, '') as estado,
                    COALESCE(estadoServicio, '') as estadoServicio,
                    COALESCE(comercial, '') as comercial,
                    COALESCE(contratar, '') as contratar,
                    COALESCE(fibra_movil, '') as fibra_movil,
                    COALESCE(lineas_adicionales, '') as lineas_adicionales,
                    COALESCE(solo_fibra, '') as solo_fibra,
                    COALESCE(solo_movil, '') as solo_movil,
                    COALESCE(nombre_cliente, '') as nombre_cliente,
                    COALESCE(dni, '') as dni,
                    COALESCE(codigo_postal, '') as codigo_postal,
                    COALESCE(email, '') as email,
                    COALESCE(telefono, '') as telefono,
                    COALESCE(iban, '') as iban,
                    COALESCE(horario_contratacion, '') as horario_contratacion,
                    fecha_creacion,
                    COALESCE(en_Comercializadora, '') as en_Comercializadora,
                    COALESCE(en_CUPSGas, '') as en_CUPSGas,
                    fecha_modificacion,
                    comision,
                    COALESCE(pdf_contrato_url, '') as pdf_contrato_url,
                    idCliente,
                    COALESCE(TV, '') as TV,
                    COALESCE(LineaMovilPrincipal, '') as LineaMovilPrincipal,
                    COALESCE(TarifaLineaMovilPrincipal, '') as TarifaLineaMovilPrincipal,
                    COALESCE(TipoOperacion, '') as TipoOperacion,
                    COALESCE(Tarifa_tel, '') as Tarifa_tel,
                    COALESCE(TipoTarifa_tel, '') as TipoTarifa_tel,
                    COALESCE(tipo_alarma, '') as tipo_alarma,
                    COALESCE(subtipo_inmueble, '') as subtipo_inmueble,
                    COALESCE(kit_alarma, '') as kit_alarma,
                    COALESCE(opcionales_alarma, '') as opcionales_alarma,
                    COALESCE(campana_alarma, '') as campana_alarma,
                    COALESCE(empresa_alarma, '') as empresa_alarma,
                    COALESCE(tipo_via_instalacion, '') as tipo_via_instalacion,
                    COALESCE(direccion_instalacion_alarma, '') as direccion_instalacion_alarma,
                    tiene_contrato_anterior,
                    COALESCE(numero_contrato_anterior, '') as numero_contrato_anterior,
                    COALESCE(compania_anterior, '') as compania_anterior,
                    fecha_permanencia_anterior,
                    usuario_comercializadora_id,
                    servicio_id,
                    potencia_contratada_p1,
                    potencia_contratada_p2,
                    consumo_ultimos_12_meses,
                    COALESCE(tipo_linea_movil_principal, '') as tipo_linea_movil_principal,
                    COALESCE(codigo_icc_principal, '') as codigo_icc_principal,
                    COALESCE(linea_movil_principal_2, '') as linea_movil_principal_2,
                    COALESCE(tipo_linea_movil_principal_2, '') as tipo_linea_movil_principal_2,
                    COALESCE(codigo_icc_principal_2, '') as codigo_icc_principal_2,
                    COALESCE(numero_instalacion, '') as numero_instalacion,
                    COALESCE(escalera_instalacion, '') as escalera_instalacion,
                    COALESCE(piso_instalacion, '') as piso_instalacion,
                    COALESCE(puerta_instalacion, '') as puerta_instalacion,
                    COALESCE(codigo_postal_instalacion, '') as codigo_postal_instalacion,
                    COALESCE(provincia_instalacion, '') as provincia_instalacion,
                    COALESCE(localidad_instalacion, '') as localidad_instalacion,
                    COALESCE(aclarador_instalacion, '') as aclarador_instalacion,
                    titular_iban_diferente,
                    COALESCE(titular_iban_dni, '') as titular_iban_dni,
                    COALESCE(titular_iban_nombre, '') as titular_iban_nombre,
                    COALESCE(titular_iban_numero, '') as titular_iban_numero,
                    fecha_activo,
                    fecha_alta,
                    COALESCE(operadora_tel, '') as operadora_tel,
                    COALESCE(fijo_tel, '') as fijo_tel,
                    COALESCE(tarifa_fibra_tel, '') as tarifa_fibra_tel,
                    numero_lineas_tel,
                    fecha_instalacion_tel,
                    COALESCE(horario_instalacion_tel, '') as horario_instalacion_tel,
                    COALESCE(telefono_linea1_tel, '') as telefono_linea1_tel,
                    COALESCE(tarifa_linea1_tel, '') as tarifa_linea1_tel,
                    COALESCE(tipo_linea1_tel, '') as tipo_linea1_tel,
                    COALESCE(codigo_icc_linea1_tel, '') as codigo_icc_linea1_tel,
                    COALESCE(telefono_linea2_tel, '') as telefono_linea2_tel,
                    COALESCE(tarifa_linea2_tel, '') as tarifa_linea2_tel,
                    COALESCE(tipo_linea2_tel, '') as tipo_linea2_tel,
                    COALESCE(codigo_icc_linea2_tel, '') as codigo_icc_linea2_tel,
                    COALESCE(telefono_linea3_tel, '') as telefono_linea3_tel,
                    COALESCE(tarifa_linea3_tel, '') as tarifa_linea3_tel,
                    COALESCE(tipo_linea3_tel, '') as tipo_linea3_tel,
                    COALESCE(codigo_icc_linea3_tel, '') as codigo_icc_linea3_tel,
                    COALESCE(telefono_linea4_tel, '') as telefono_linea4_tel,
                    COALESCE(tarifa_linea4_tel, '') as tarifa_linea4_tel,
                    COALESCE(tipo_linea4_tel, '') as tipo_linea4_tel,
                    COALESCE(codigo_icc_linea4_tel, '') as codigo_icc_linea4_tel,
                    COALESCE(telefono_linea5_tel, '') as telefono_linea5_tel,
                    COALESCE(tarifa_linea5_tel, '') as tarifa_linea5_tel,
                    COALESCE(tipo_linea5_tel, '') as tipo_linea5_tel,
                    COALESCE(codigo_icc_linea5_tel, '') as codigo_icc_linea5_tel,
                    COALESCE(direccion, '') as direccion,
                    COALESCE(observaciones, '') as observaciones,
                    COALESCE(en_Tarifa, '') as en_Tarifa,
                    COALESCE(en_CUPS, '') as en_CUPS,
                    COALESCE(en_Servicios, '') as en_Servicios,
                    COALESCE(en_IBAN, '') as en_IBAN,
                    COALESCE(observaciones_alarma, '') as observaciones_alarma,
                    COALESCE(observaciones_estado, '') as observaciones_estado,
                    COALESCE(direccion_segunda_residencia, '') as direccion_segunda_residencia,
                    COALESCE(tarifa_fibra_segunda_residencia, '') as tarifa_fibra_segunda_residencia
                FROM contratos";
            
            var whereConditions = new List<string>();
            var parameters = new List<object>();
            
            // Filtrar por tipo
            if (!string.IsNullOrEmpty(tipo))
            {
                whereConditions.Add($"tipo = {{{parameters.Count}}}");
                parameters.Add(tipo);
            }
            
            // Filtrar por rol
            if (!string.IsNullOrEmpty(rolUsuario) && !string.IsNullOrEmpty(nombreUsuario))
            {
                if (rolUsuario == "Colaborador")
                {
                    whereConditions.Add($"comercial = {{{parameters.Count}}}");
                    parameters.Add(nombreUsuario);
                }
                else if (rolUsuario == "Gestor" && usuarioId.HasValue)
                {
                    // El gestor ve sus propios contratos y los de sus colaboradores
                    var colaboradores = await context.Usuarios
                        .Where(u => u.GestorId == usuarioId.Value)
                        .Select(u => u.NombreUsuario)
                        .ToListAsync();
                    
                    // Añadir el propio nombre del gestor a la lista
                    var nombresUsuarios = new List<string> { nombreUsuario };
                    nombresUsuarios.AddRange(colaboradores);
                    
                    // Crear condición IN para múltiples usuarios
                    var inConditions = string.Join(" OR ", nombresUsuarios.Select((_, i) => $"comercial = {{{parameters.Count + i}}}"));
                    whereConditions.Add($"({inConditions})");
                    parameters.AddRange(nombresUsuarios);
                }
                else if (rolUsuario == "Jefe de ventas" && usuarioId.HasValue)
                {
                    // El jefe de ventas ve:
                    // 1. Sus propios contratos
                    // 2. Los contratos de sus colaboradores directos
                    // 3. Los contratos de sus gestores asignados
                    // 4. Los contratos de los colaboradores de esos gestores
                    
                    var nombresUsuarios = new List<string> { nombreUsuario };
                    
                    // Obtener colaboradores directos del jefe de ventas
                    var colaboradoresDirectos = await context.Usuarios
                        .Where(u => u.JefeVentasId == usuarioId.Value && u.Rol == "Colaborador")
                        .Select(u => u.NombreUsuario)
                        .ToListAsync();
                    nombresUsuarios.AddRange(colaboradoresDirectos);
                    
                    // Obtener gestores del jefe de ventas
                    var gestores = await context.Usuarios
                        .Where(u => u.JefeVentasId == usuarioId.Value && u.Rol == "Gestor")
                        .ToListAsync();
                    
                    foreach (var gestor in gestores)
                    {
                        // Añadir el gestor
                        nombresUsuarios.Add(gestor.NombreUsuario);
                        
                        // Añadir los colaboradores de este gestor
                        var colaboradoresGestor = await context.Usuarios
                            .Where(u => u.GestorId == gestor.Id)
                            .Select(u => u.NombreUsuario)
                            .ToListAsync();
                        nombresUsuarios.AddRange(colaboradoresGestor);
                    }
                    
                    // Crear condición IN para todos los usuarios
                    var inConditions = string.Join(" OR ", nombresUsuarios.Select((_, i) => $"comercial = {{{parameters.Count + i}}}"));
                    whereConditions.Add($"({inConditions})");
                    parameters.AddRange(nombresUsuarios);
                }
                else if (rolUsuario == "Director comercial" && usuarioId.HasValue)
                {
                    // El director comercial ve:
                    // 1. Sus propios contratos
                    // 2. Los contratos de sus jefes de ventas asignados
                    // 3. Los contratos de los colaboradores y gestores de esos jefes de ventas
                    // 4. Los contratos de sus gestores asignados directamente
                    // 5. Los contratos de los colaboradores de esos gestores
                    // 6. Los contratos de sus colaboradores asignados directamente
                    
                    var nombresUsuarios = new List<string> { nombreUsuario };
                    
                    // Obtener jefes de ventas del director comercial
                    var jefesVentas = await context.Usuarios
                        .Where(u => u.DirectorComercialId == usuarioId.Value && u.Rol == "Jefe de ventas")
                        .ToListAsync();
                    
                    foreach (var jefeVentas in jefesVentas)
                    {
                        // Añadir el jefe de ventas
                        nombresUsuarios.Add(jefeVentas.NombreUsuario);
                        
                        // Obtener colaboradores directos del jefe de ventas
                        var colaboradoresJefe = await context.Usuarios
                            .Where(u => u.JefeVentasId == jefeVentas.Id && u.Rol == "Colaborador")
                            .Select(u => u.NombreUsuario)
                            .ToListAsync();
                        nombresUsuarios.AddRange(colaboradoresJefe);
                        
                        // Obtener gestores del jefe de ventas
                        var gestoresJefe = await context.Usuarios
                            .Where(u => u.JefeVentasId == jefeVentas.Id && u.Rol == "Gestor")
                            .ToListAsync();
                        
                        foreach (var gestor in gestoresJefe)
                        {
                            // Añadir el gestor
                            nombresUsuarios.Add(gestor.NombreUsuario);
                            
                            // Añadir los colaboradores de este gestor
                            var colaboradoresGestor = await context.Usuarios
                                .Where(u => u.GestorId == gestor.Id)
                                .Select(u => u.NombreUsuario)
                                .ToListAsync();
                            nombresUsuarios.AddRange(colaboradoresGestor);
                        }
                    }
                    
                    // Obtener gestores asignados directamente al director comercial
                    var gestoresDirectos = await context.Usuarios
                        .Where(u => u.DirectorComercialId == usuarioId.Value && u.Rol == "Gestor")
                        .ToListAsync();
                    
                    foreach (var gestor in gestoresDirectos)
                    {
                        // Añadir el gestor
                        nombresUsuarios.Add(gestor.NombreUsuario);
                        
                        // Añadir los colaboradores de este gestor
                        var colaboradoresGestor = await context.Usuarios
                            .Where(u => u.GestorId == gestor.Id)
                            .Select(u => u.NombreUsuario)
                            .ToListAsync();
                        nombresUsuarios.AddRange(colaboradoresGestor);
                    }
                    
                    // Obtener colaboradores asignados directamente al director comercial
                    var colaboradoresDirectos = await context.Usuarios
                        .Where(u => u.DirectorComercialId == usuarioId.Value && u.Rol == "Colaborador")
                        .Select(u => u.NombreUsuario)
                        .ToListAsync();
                    nombresUsuarios.AddRange(colaboradoresDirectos);
                    
                    // Eliminar duplicados
                    nombresUsuarios = nombresUsuarios.Distinct().ToList();
                    
                    // Crear condición IN para todos los usuarios
                    var inConditions = string.Join(" OR ", nombresUsuarios.Select((_, i) => $"comercial = {{{parameters.Count + i}}}"));
                    whereConditions.Add($"({inConditions})");
                    parameters.AddRange(nombresUsuarios);
                }
                else if (rolUsuario == "Comercializadora")
                {
                    var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
                    if (usuario != null && !string.IsNullOrEmpty(usuario.Comercializadora))
                    {
                        whereConditions.Add($"en_Comercializadora = {{{parameters.Count}}}");
                        parameters.Add(usuario.Comercializadora);
                    }
                }
            }
            
            if (whereConditions.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", whereConditions);
            }
            
            sql += " ORDER BY COALESCE(fecha_modificacion, fecha_creacion, '1900-01-01') DESC";
            
            var contratos = await context.Contratos
                .FromSqlRaw(sql, parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();
            
            // Cargar los nombres de cliente para los que tengan IdCliente pero nombre_cliente vacío
            var idsClientes = contratos
                .Where(c => c.IdCliente.HasValue && string.IsNullOrEmpty(c.NombreCliente))
                .Select(c => c.IdCliente!.Value)
                .Distinct()
                .ToList();
            
            if (idsClientes.Any())
            {
                var clientes = await context.Clientes
                    .Where(c => idsClientes.Contains(c.Id))
                    .Select(c => new { c.Id, c.Nombre })
                    .ToListAsync();
                
                var diccionarioClientes = clientes.ToDictionary(c => c.Id, c => c.Nombre);
                
                foreach (var contrato in contratos.Where(c => c.IdCliente.HasValue && string.IsNullOrEmpty(c.NombreCliente)))
                {
                    if (diccionarioClientes.TryGetValue(contrato.IdCliente!.Value, out var nombreCliente))
                    {
                        contrato.NombreCliente = nombreCliente;
                    }
                }
            }
            
            return contratos;
        }

        public async Task<Contrato?> ObtenerPorIdAsync(int id)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            // Cargar el contrato de forma raw y luego manejar nulls en memoria
            var query = context.Contratos.Where(c => c.Id == id);
            var contratos = await query.ToListAsync();
            
            if (!contratos.Any())
                return null;
            
            var contrato = contratos.First();
            
            // Normalizar todos los campos string NULL a cadena vacía
            contrato.Tipo ??= string.Empty;
            contrato.Estado ??= string.Empty;
            contrato.Comercial ??= string.Empty;
            contrato.NombreCliente ??= string.Empty;
            contrato.Dni ??= string.Empty;
            contrato.Direccion ??= string.Empty;
            contrato.Iban ??= string.Empty;
            contrato.EstadoServicio ??= string.Empty;
            contrato.EnComercializadora ??= string.Empty;
            contrato.EnTarifa ??= string.Empty;
            contrato.EnCups ??= string.Empty;
            contrato.EnCupsGas ??= string.Empty;
            contrato.EnServicios ??= string.Empty;
            contrato.EnIban ??= string.Empty;
            contrato.TipoOperacion ??= string.Empty;
            contrato.OperadoraTel ??= string.Empty;
            contrato.TarifaTel ??= string.Empty;
            contrato.TipoTarifaTel ??= string.Empty;
            contrato.FijoTel ??= string.Empty;
            contrato.LineaMovilPrincipal ??= string.Empty;
            contrato.TipoLineaMovilPrincipal ??= string.Empty;
            contrato.CodigoIccPrincipal ??= string.Empty;
            contrato.TelefonoLinea1Tel ??= string.Empty;
            contrato.TarifaLinea1Tel ??= string.Empty;
            contrato.TipoLinea1Tel ??= string.Empty;
            contrato.CodigoIccLinea1Tel ??= string.Empty;
            contrato.TelefonoLinea2Tel ??= string.Empty;
            contrato.TarifaLinea2Tel ??= string.Empty;
            contrato.TipoLinea2Tel ??= string.Empty;
            contrato.CodigoIccLinea2Tel ??= string.Empty;
            contrato.TelefonoLinea3Tel ??= string.Empty;
            contrato.TarifaLinea3Tel ??= string.Empty;
            contrato.TipoLinea3Tel ??= string.Empty;
            contrato.CodigoIccLinea3Tel ??= string.Empty;
            contrato.TelefonoLinea4Tel ??= string.Empty;
            contrato.TarifaLinea4Tel ??= string.Empty;
            contrato.TipoLinea4Tel ??= string.Empty;
            contrato.CodigoIccLinea4Tel ??= string.Empty;
            contrato.TelefonoLinea5Tel ??= string.Empty;
            contrato.TarifaLinea5Tel ??= string.Empty;
            contrato.TipoLinea5Tel ??= string.Empty;
            contrato.CodigoIccLinea5Tel ??= string.Empty;
            contrato.HorarioInstalacionTel ??= string.Empty;
            contrato.Contratar ??= string.Empty;
            contrato.Tv ??= string.Empty;
            contrato.TipoAlarma ??= string.Empty;
            contrato.SubtipoInmueble ??= string.Empty;
            contrato.CompaniaAnterior ??= string.Empty;
            contrato.NumeroContratoAnterior ??= string.Empty;
            contrato.KitAlarma ??= string.Empty;
            contrato.OpcionalesAlarma ??= string.Empty;
            contrato.CampanaAlarma ??= string.Empty;
            contrato.EmpresaAlarma ??= string.Empty;
            contrato.DireccionInstalacionAlarma ??= string.Empty;
            contrato.NumeroInstalacion ??= string.Empty;
            contrato.EscaleraInstalacion ??= string.Empty;
            contrato.PisoInstalacion ??= string.Empty;
            contrato.PuertaInstalacion ??= string.Empty;
            contrato.CodigoPostalInstalacion ??= string.Empty;
            contrato.ProvinciaInstalacion ??= string.Empty;
            contrato.LocalidadInstalacion ??= string.Empty;
            contrato.AclaradorInstalacion ??= string.Empty;
            contrato.ObservacionesAlarma ??= string.Empty;
            contrato.ObservacionesEstado ??= string.Empty;
            contrato.TitularIbanDni ??= string.Empty;
            contrato.TitularIbanNombre ??= string.Empty;
            contrato.TitularIbanNumero ??= string.Empty;
            contrato.PdfContratoUrl ??= string.Empty;
            
            return contrato;
        }

        public async Task<bool> ActualizarAsync(Contrato contrato)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                // Obtener el contrato existente
                var contratoExistente = await context.Contratos.FindAsync(contrato.Id);
                if (contratoExistente == null) return false;

                // Preservar la fecha de creación original
                var fechaCreacionOriginal = contratoExistente.FechaCreacion;
                
                // Actualizar fecha de modificación
                contrato.FechaModificacion = DateTime.Now;
                contrato.FechaCreacion = fechaCreacionOriginal;

                // Log para depuración detallada
                Console.WriteLine($"=== ACTUALIZANDO CONTRATO {contrato.Id} ===");
                Console.WriteLine($"NumeroInstalacion: '{contrato.NumeroInstalacion}'");
                Console.WriteLine($"EscaleraInstalacion: '{contrato.EscaleraInstalacion}'");
                Console.WriteLine($"PisoInstalacion: '{contrato.PisoInstalacion}'");
                Console.WriteLine($"PuertaInstalacion: '{contrato.PuertaInstalacion}'");
                Console.WriteLine($"CodigoPostalInstalacion: '{contrato.CodigoPostalInstalacion}'");
                Console.WriteLine($"ProvinciaInstalacion: '{contrato.ProvinciaInstalacion}'");
                Console.WriteLine($"LocalidadInstalacion: '{contrato.LocalidadInstalacion}'");
                Console.WriteLine($"AclaradorInstalacion: '{contrato.AclaradorInstalacion}'");
                Console.WriteLine($"FechaPermanenciaAnterior: {contrato.FechaPermanenciaAnterior}");

                // Actualizar todas las propiedades
                context.Entry(contratoExistente).CurrentValues.SetValues(contrato);
                
                await context.SaveChangesAsync();
                
                Console.WriteLine("Contrato actualizado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar contrato: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> CrearAsync(Contrato contrato)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var ahora = DateTime.Now;
                contrato.FechaCreacion = ahora;
                contrato.FechaModificacion = ahora;
                
                // Log para depuración detallada
                Console.WriteLine($"=== CREANDO NUEVO CONTRATO ===");
                Console.WriteLine($"NumeroInstalacion: '{contrato.NumeroInstalacion}'");
                Console.WriteLine($"EscaleraInstalacion: '{contrato.EscaleraInstalacion}'");
                Console.WriteLine($"PisoInstalacion: '{contrato.PisoInstalacion}'");
                Console.WriteLine($"PuertaInstalacion: '{contrato.PuertaInstalacion}'");
                Console.WriteLine($"CodigoPostalInstalacion: '{contrato.CodigoPostalInstalacion}'");
                Console.WriteLine($"ProvinciaInstalacion: '{contrato.ProvinciaInstalacion}'");
                Console.WriteLine($"LocalidadInstalacion: '{contrato.LocalidadInstalacion}'");
                Console.WriteLine($"AclaradorInstalacion: '{contrato.AclaradorInstalacion}'");
                Console.WriteLine($"FechaPermanenciaAnterior: {contrato.FechaPermanenciaAnterior}");
                
                await context.Contratos.AddAsync(contrato);
                await context.SaveChangesAsync();
                
                Console.WriteLine($"Contrato creado con ID: {contrato.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear contrato: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                await using var context = _dbContextProvider.CreateDbContext();
                var contrato = await context.Contratos.FindAsync(id);
                if (contrato == null) return false;

                context.Contratos.Remove(contrato);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> ObtenerComercialesAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Usuarios
                .Where(u => !string.IsNullOrEmpty(u.NombreUsuario))
                .Select(u => u.NombreUsuario)
                .Distinct()
                .OrderBy(u => u)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerComercializadorasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Contratos
                .Where(c => !string.IsNullOrEmpty(c.EnComercializadora))
                .Select(c => c.EnComercializadora!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerOperadorasAsync()
        {
            await using var context = _dbContextProvider.CreateDbContext();
            return await context.Contratos
                .Where(c => !string.IsNullOrEmpty(c.OperadoraTel))
                .Select(c => c.OperadoraTel!)
                .Distinct()
                .OrderBy(o => o)
                .ToListAsync();
        }

        public static string ObtenerComercializadoraParaVisualizacion(string? comercializadora)
        {
            if (string.IsNullOrWhiteSpace(comercializadora))
                return string.Empty;

            var valor = comercializadora.Trim();
            
            // Mostrar "Iberdrola" para e-iberdrola y a-iberdrola
            if (valor.Equals("e-iberdrola", StringComparison.OrdinalIgnoreCase) ||
                valor.Equals("a-iberdrola", StringComparison.OrdinalIgnoreCase))
            {
                return "Iberdrola";
            }

            return valor;
        }
    }
}
