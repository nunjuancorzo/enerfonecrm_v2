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
            
            Console.WriteLine($"[DEBUG ContratoService] ObtenerTodosPorTipoAsync llamado");
            Console.WriteLine($"[DEBUG ContratoService] Tipo: {tipo}");
            Console.WriteLine($"[DEBUG ContratoService] Rol: {rolUsuario}");
            Console.WriteLine($"[DEBUG ContratoService] Usuario: {nombreUsuario}");
            Console.WriteLine($"[DEBUG ContratoService] UsuarioId: {usuarioId}");
            
            // Todos los campos VARCHAR/TEXT envueltos en COALESCE para evitar NULL casting
            var sql = @"
                SELECT 
                    id, 
                    COALESCE(id_contrato_externo, '') as id_contrato_externo,
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
                    tarifa_tel_id,
                    COALESCE(TipoTarifa_tel, '') as TipoTarifa_tel,
                    COALESCE(tipo_alarma, '') as tipo_alarma,
                    COALESCE(subtipo_inmueble, '') as subtipo_inmueble,
                    COALESCE(kit_alarma, '') as kit_alarma,
                    kit_alarma_id,
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
                    historico_liquidacion_id,
                    potencia_contratada_p1,
                    potencia_contratada_p2,
                    consumo_ultimos_12_meses,
                    consumo_anual_gas,
                    COALESCE(peaje_luz, '') as peaje_luz,
                    COALESCE(peaje_gas, '') as peaje_gas,
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
                    COALESCE(titular_iban_diferente, FALSE) as titular_iban_diferente,
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
                    COALESCE(telefono_linea6_tel, '') as telefono_linea6_tel,
                    COALESCE(tarifa_linea6_tel, '') as tarifa_linea6_tel,
                    COALESCE(tipo_linea6_tel, '') as tipo_linea6_tel,
                    COALESCE(codigo_icc_linea6_tel, '') as codigo_icc_linea6_tel,
                    COALESCE(telefono_linea7_tel, '') as telefono_linea7_tel,
                    COALESCE(tarifa_linea7_tel, '') as tarifa_linea7_tel,
                    COALESCE(tipo_linea7_tel, '') as tipo_linea7_tel,
                    COALESCE(codigo_icc_linea7_tel, '') as codigo_icc_linea7_tel,
                    COALESCE(telefono_linea8_tel, '') as telefono_linea8_tel,
                    COALESCE(tarifa_linea8_tel, '') as tarifa_linea8_tel,
                    COALESCE(tipo_linea8_tel, '') as tipo_linea8_tel,
                    COALESCE(codigo_icc_linea8_tel, '') as codigo_icc_linea8_tel,
                    COALESCE(telefono_linea9_tel, '') as telefono_linea9_tel,
                    COALESCE(tarifa_linea9_tel, '') as tarifa_linea9_tel,
                    COALESCE(tipo_linea9_tel, '') as tipo_linea9_tel,
                    COALESCE(codigo_icc_linea9_tel, '') as codigo_icc_linea9_tel,
                    COALESCE(telefono_linea10_tel, '') as telefono_linea10_tel,
                    COALESCE(tarifa_linea10_tel, '') as tarifa_linea10_tel,
                    COALESCE(tipo_linea10_tel, '') as tipo_linea10_tel,
                    COALESCE(codigo_icc_linea10_tel, '') as codigo_icc_linea10_tel,
                    COALESCE(telefono_linea11_tel, '') as telefono_linea11_tel,
                    COALESCE(tarifa_linea11_tel, '') as tarifa_linea11_tel,
                    COALESCE(tipo_linea11_tel, '') as tipo_linea11_tel,
                    COALESCE(codigo_icc_linea11_tel, '') as codigo_icc_linea11_tel,
                    COALESCE(telefono_linea12_tel, '') as telefono_linea12_tel,
                    COALESCE(tarifa_linea12_tel, '') as tarifa_linea12_tel,
                    COALESCE(tipo_linea12_tel, '') as tipo_linea12_tel,
                    COALESCE(codigo_icc_linea12_tel, '') as codigo_icc_linea12_tel,
                    COALESCE(telefono_linea13_tel, '') as telefono_linea13_tel,
                    COALESCE(tarifa_linea13_tel, '') as tarifa_linea13_tel,
                    COALESCE(tipo_linea13_tel, '') as tipo_linea13_tel,
                    COALESCE(codigo_icc_linea13_tel, '') as codigo_icc_linea13_tel,
                    COALESCE(telefono_linea14_tel, '') as telefono_linea14_tel,
                    COALESCE(tarifa_linea14_tel, '') as tarifa_linea14_tel,
                    COALESCE(tipo_linea14_tel, '') as tipo_linea14_tel,
                    COALESCE(codigo_icc_linea14_tel, '') as codigo_icc_linea14_tel,
                    COALESCE(telefono_linea15_tel, '') as telefono_linea15_tel,
                    COALESCE(tarifa_linea15_tel, '') as tarifa_linea15_tel,
                    COALESCE(tipo_linea15_tel, '') as tipo_linea15_tel,
                    COALESCE(codigo_icc_linea15_tel, '') as codigo_icc_linea15_tel,
                    COALESCE(direccion, '') as direccion,
                    COALESCE(observaciones, '') as observaciones,
                    COALESCE(en_Tarifa, '') as en_Tarifa,
                    en_tarifa_id,
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
            
            Console.WriteLine($"[DEBUG ContratoService] SQL Query: {sql}");
            Console.WriteLine($"[DEBUG ContratoService] Parámetros: {string.Join(", ", parameters)}");
            Console.WriteLine($"[DEBUG ContratoService] Condiciones WHERE: {whereConditions.Count}");
            
            var contratos = await context.Contratos
                .FromSqlRaw(sql, parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();
            
            Console.WriteLine($"[DEBUG ContratoService] Contratos devueltos: {contratos.Count}");
            
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
            
            // Log temporal para depuración
            Console.WriteLine($"[DEBUG] Contrato ID {contrato.Id} cargado - IdContratoExterno: '{contrato.IdContratoExterno}' (es null: {contrato.IdContratoExterno == null})");
            Console.WriteLine($"[DEBUG] ConsumoAnual (Luz): {contrato.ConsumoAnual}");
            Console.WriteLine($"[DEBUG] ConsumoAnualGas: {contrato.ConsumoAnualGas}");
            
            // Normalizar todos los campos string NULL a cadena vacía
            contrato.IdContratoExterno ??= string.Empty;
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
                Console.WriteLine($"ConsumoAnual (Luz): {contrato.ConsumoAnual}");
                Console.WriteLine($"ConsumoAnualGas: {contrato.ConsumoAnualGas}");

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
                Console.WriteLine($"ConsumoAnual (Luz): {contrato.ConsumoAnual}");
                Console.WriteLine($"ConsumoAnualGas: {contrato.ConsumoAnualGas}");
                
                await context.Contratos.AddAsync(contrato);
                await context.SaveChangesAsync();
                
                Console.WriteLine($"Contrato creado con ID: {contrato.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear contrato: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack Trace: {ex.InnerException.StackTrace}");
                }
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

        /// <summary>
        /// Da de baja un contrato y verifica si requiere decomisión por baja anticipada
        /// </summary>
        /// <param name="contratoId">ID del contrato a dar de baja</param>
        /// <param name="comisionService">Servicio de comisiones para verificar penalizaciones</param>
        /// <param name="usuarioActualId">ID del usuario que realiza la baja</param>
        /// <param name="fechaBaja">Fecha de baja (opcional, por defecto hoy)</param>
        /// <param name="observaciones">Observaciones sobre la baja</param>
        /// <returns>(éxito, decomisión creada si aplica)</returns>
        public async Task<(bool exito, Decomision? decomision)> DarDeBajaContratoAsync(
            int contratoId,
            ComisionService comisionService,
            int? usuarioActualId = null,
            DateTime? fechaBaja = null,
            string? observaciones = null)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            var contrato = await context.Contratos.FindAsync(contratoId);
            if (contrato == null)
                return (false, null);

            fechaBaja ??= DateTime.Now;

            // Verificar si requiere decomisión
            var (requierePenalizacion, diasPenalizacion, tipoPenalizacion) = 
                await comisionService.VerificarPenalizacionAsync(contrato, fechaBaja);

            Decomision? decomision = null;

            if (requierePenalizacion)
            {
                // Buscar el usuario que registró el contrato
                var usuario = await context.Usuarios
                    .FirstOrDefaultAsync(u => u.NombreUsuario == contrato.Comercial);

                if (usuario != null)
                {
                    // Crear la decomisión (liquidación null si aún no se ha liquidado)
                    decomision = await comisionService.CrearDecomisionAsync(
                        contrato,
                        usuario,
                        fechaBaja.Value,
                        contrato.HistoricoLiquidacionId,  // Puede ser null si aún no se liquidó
                        usuarioActualId,
                        observaciones
                    );
                    
                    Console.WriteLine($"[DEBUG ContratoService] Decomisión creada: ContratoId={contrato.Id}, UsuarioId={usuario.Id}, Importe={decomision?.ImporteDecomision}");
                }
            }

            // Actualizar el estado del contrato a "Baja"
            contrato.Estado = "Baja";
            contrato.FechaModificacion = DateTime.Now;
            
            context.Contratos.Update(contrato);
            await context.SaveChangesAsync();

            return (true, decomision);
        }

        /// <summary>
        /// Verifica si un contrato puede darse de baja sin penalización
        /// </summary>
        public async Task<(bool puedeRechazar, int? diasPendientes, decimal? importePenalizacion)> 
            VerificarPenalizacionBajaAsync(int contratoId, ComisionService comisionService)
        {
            await using var context = _dbContextProvider.CreateDbContext();

            Console.WriteLine($"[DEBUG ContratoService] Verificando penalización de baja para contrato {contratoId}");

            var contrato = await context.Contratos.FindAsync(contratoId);
            if (contrato == null)
            {
                Console.WriteLine($"[DEBUG] Contrato no encontrado");
                return (false, null, null);
            }

            Console.WriteLine($"[DEBUG] Contrato encontrado: Tipo={contrato.Tipo}, EnTarifaId={contrato.EnTarifaId}, EnTarifa={contrato.EnTarifa}, FechaActivo={contrato.FechaActivo?.ToString("yyyy-MM-dd")}");

            var (requierePenalizacion, diasPenalizacion, tipoPenalizacion) = 
                await comisionService.VerificarPenalizacionAsync(contrato);

            Console.WriteLine($"[DEBUG] Resultado VerificarPenalizacionAsync: requiere={requierePenalizacion}, dias={diasPenalizacion}, tipo={tipoPenalizacion}");

            if (!requierePenalizacion || !diasPenalizacion.HasValue)
            {
                Console.WriteLine($"[DEBUG] Sin penalización requerida o dias no definidos - puedeRechazar=true");
                return (true, 0, 0);
            }

            // Usar FechaActivo (cuando pasó a estado Activo) en lugar de FechaAlta
            var fechaActivacion = contrato.FechaActivo ?? DateTime.Now.AddDays(-diasPenalizacion.Value - 1);
            var diasActivo = (DateTime.Now - fechaActivacion).Days;
            var diasPendientes = Math.Max(0, diasPenalizacion.Value - diasActivo);

            Console.WriteLine($"[DEBUG] Cálculo días: fechaActivacion={fechaActivacion:yyyy-MM-dd}, diasActivo={diasActivo}, diasRequeridos={diasPenalizacion}, diasPendientes={diasPendientes}");

            decimal importePenalizacion;
            var comisionOriginal = contrato.Comision ?? 0;

            if (tipoPenalizacion == "Proporcional")
            {
                importePenalizacion = Math.Round((comisionOriginal * diasPendientes) / diasPenalizacion.Value, 2);
                Console.WriteLine($"[DEBUG] Penalización Proporcional: comision={comisionOriginal}, importe={importePenalizacion}");
            }
            else
            {
                importePenalizacion = comisionOriginal;
                Console.WriteLine($"[DEBUG] Penalización Total: comision={comisionOriginal}, importe={importePenalizacion}");
            }

            Console.WriteLine($"[DEBUG] Retornando: puedeRechazar=false, diasPendientes={diasPendientes}, importePenalizacion={importePenalizacion}");

            return (false, diasPendientes, importePenalizacion);
        }

        /// <summary>
        /// Recalcula las comisiones de todos los contratos con una tarifa específica
        /// que estén en estados válidos para actualización
        /// </summary>
        /// <param name="tarifaId">ID de la tarifa</param>
        /// <param name="tipoTarifa">Tipo: "Energia", "Telefonia" o "Alarma"</param>
        /// <param name="nuevaComision">Nueva comisión de la tarifa</param>
        /// <param name="usuarioService">Servicio de usuarios para obtener porcentajes</param>
        /// <param name="comisionService">Servicio de comisiones para obtener configuración específica</param>
        /// <returns>Número de contratos actualizados</returns>
        public async Task<int> RecalcularComisionesPorTarifaAsync(
            int tarifaId, 
            string tipoTarifa, 
            decimal nuevaComision,
            UsuarioService usuarioService,
            ComisionService comisionService)
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            // Estados en los que se debe recalcular la comisión
            var estadosValidos = new List<string>
            {
                "Pte Carga",
                "Solicitado",
                "Pte Firma",
                "En incidencia",
                "Pte Documentación",
                "Pte Validación",
                "En Curso",
                "En Activación",
                "En tramitación",
                "Activo",
                "Act/Facturable"
            };

            // Obtener el nombre de la tarifa para buscar también por nombre
            string? nombreTarifa = null;
            
            switch (tipoTarifa)
            {
                case "Energia":
                    var tarifaEnergia = await context.TarifasEnergia.FindAsync(tarifaId);
                    nombreTarifa = tarifaEnergia?.Nombre;
                    break;
                case "Telefonia":
                    var tarifaTelefonia = await context.TarifasTelefonia.FindAsync(tarifaId);
                    nombreTarifa = tarifaTelefonia?.Tarifa;
                    break;
                case "Alarma":
                    var tarifaAlarma = await context.TarifasAlarmas.FindAsync(tarifaId);
                    nombreTarifa = tarifaAlarma?.NombreTarifa;
                    break;
            }

            // Buscar contratos según el tipo de tarifa (por ID o por nombre)
            List<Contrato> contratos;
            
            switch (tipoTarifa)
            {
                case "Energia":
                    contratos = await context.Contratos
                        .Where(c => estadosValidos.Contains(c.Estado) && 
                               (c.EnTarifaId == tarifaId || 
                                (!string.IsNullOrEmpty(nombreTarifa) && c.EnTarifa == nombreTarifa)))
                        .ToListAsync();
                    break;
                    
                case "Telefonia":
                    contratos = await context.Contratos
                        .Where(c => estadosValidos.Contains(c.Estado) && 
                               (c.TarifaTelId == tarifaId || 
                                (!string.IsNullOrEmpty(nombreTarifa) && c.TarifaTel == nombreTarifa)))
                        .ToListAsync();
                    break;
                    
                case "Alarma":
                    contratos = await context.Contratos
                        .Where(c => estadosValidos.Contains(c.Estado) && 
                               (c.KitAlarmaId == tarifaId || 
                                (!string.IsNullOrEmpty(nombreTarifa) && c.KitAlarma == nombreTarifa)))
                        .ToListAsync();
                    break;
                    
                default:
                    return 0;
            }

            int contratosActualizados = 0;

            foreach (var contrato in contratos)
            {
                decimal comisionFinal = nuevaComision;

                // Intentar obtener el usuario: primero por ID, luego por nombre del comercial
                Usuario? usuario = null;
                
                if (contrato.UsuarioComercializadoraId.HasValue)
                {
                    usuario = await usuarioService.ObtenerPorIdAsync(contrato.UsuarioComercializadoraId.Value);
                }
                else if (!string.IsNullOrEmpty(contrato.Comercial))
                {
                    // Buscar usuario por nombre de usuario (username)
                    usuario = await context.Usuarios
                        .FirstOrDefaultAsync(u => u.NombreUsuario == contrato.Comercial);
                }
                
                // Obtener configuración de comisión específica para usuario + proveedor
                if (usuario != null)
                {
                    // Obtener el nombre del proveedor según el tipo de tarifa
                    string? nombreProveedor = tipoTarifa switch
                    {
                        "Energia" => contrato.EnComercializadora,
                        "Telefonia" => contrato.OperadoraTel,
                        "Alarma" => contrato.EmpresaAlarma,
                        _ => null
                    };

                    // Buscar el ID del proveedor por nombre
                    int? proveedorId = null;
                    if (!string.IsNullOrEmpty(nombreProveedor))
                    {
                        proveedorId = tipoTarifa switch
                        {
                            "Energia" => (await context.Comercializadoras
                                .FirstOrDefaultAsync(c => c.Nombre == nombreProveedor))?.Id,
                            "Telefonia" => (await context.Operadoras
                                .FirstOrDefaultAsync(o => o.Nombre == nombreProveedor))?.Id,
                            "Alarma" => (await context.EmpresasAlarmas
                                .FirstOrDefaultAsync(e => e.Nombre == nombreProveedor))?.Id,
                            _ => null
                        };
                    }

                    // Obtener la configuración de comisión para este usuario + proveedor
                    if (proveedorId.HasValue)
                    {
                        string tipoProveedor = tipoTarifa switch
                        {
                            "Energia" => "Comercializadora",
                            "Telefonia" => "Operadora",
                            "Alarma" => "EmpresaAlarma",
                            _ => ""
                        };

                        var configuracion = await comisionService.ObtenerConfiguracionAsync(
                            usuario.Id, 
                            tipoProveedor, 
                            proveedorId.Value);

                        if (configuracion != null && configuracion.PorcentajeColaborador > 0)
                        {
                            comisionFinal = nuevaComision * (configuracion.PorcentajeColaborador / 100);
                            Console.WriteLine($"[ContratoService] Contrato {contrato.Id}: Aplicando {configuracion.PorcentajeColaborador}% de {nombreProveedor} = {comisionFinal}€");
                        }
                        else
                        {
                            Console.WriteLine($"[ContratoService] Contrato {contrato.Id}: No se encontró configuración de comisión para {usuario.NombreUsuario} + {nombreProveedor}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ContratoService] Contrato {contrato.Id}: No se pudo determinar el proveedor ID (nombre: {nombreProveedor})");
                    }
                }

                contrato.Comision = comisionFinal;
                contrato.FechaModificacion = DateTime.Now;
                contratosActualizados++;
            }

            if (contratosActualizados > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"[ContratoService] Recalculadas comisiones de {contratosActualizados} contratos de tipo {tipoTarifa} con tarifa ID {tarifaId} (nombre: {nombreTarifa})");
            }

            return contratosActualizados;
        }
    }
}
