using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EnerfoneCRM.Services;

public class UsuarioService
{
    private readonly DbContextProvider _dbContextProvider;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(DbContextProvider dbContextProvider, ILogger<UsuarioService> logger)
    {
        _dbContextProvider = dbContextProvider;
        _logger = logger;
    }

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var usuarios = await context.Usuarios
            .OrderByDescending(u => u.FechaCreacion)
            .ToListAsync();

        // Cargar el conteo de contratos para cada usuario
        // Los contratos tienen el campo "comercial" que contiene el nombre del usuario
        foreach (var usuario in usuarios)
        {
            // Contar todos los contratos donde el campo comercial coincide con el nombre de usuario
            usuario.TotalContratos = await context.Contratos
                .Where(c => c.Comercial == usuario.NombreUsuario)
                .CountAsync();
        }

        // Excluir superadmin del listado (nunca debe aparecer)
        return usuarios.Where(u => u.Id != -1 && u.NombreUsuario != "superadmin").ToList();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<List<Usuario>> ObtenerAdministradoresActivosAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.Usuarios
            .Where(u => u.Rol == "Administrador" && u.Activo && !string.IsNullOrEmpty(u.Email))
            .ToListAsync();
    }

    public async Task<(bool exito, string mensaje)> CrearAsync(Usuario usuario, string password)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Validar que solo puede haber un administrador
        if (usuario.Rol == "Administrador")
        {
            var existeAdministrador = await context.Usuarios
                .AnyAsync(u => u.Rol == "Administrador");
            
            if (existeAdministrador)
            {
                return (false, "Ya existe un usuario con rol Administrador. Solo puede haber un administrador por sistema.");
            }
        }
        
        var existeNombreUsuario = await context.Usuarios
            .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario);

        if (existeNombreUsuario)
        {
            return (false, "Ya existe un usuario con este nombre de usuario");
        }

        var existeEmail = await context.Usuarios
            .AnyAsync(u => u.Email == usuario.Email);

        if (existeEmail)
        {
            return (false, "Ya existe un usuario con este email");
        }

        // Contraseña en texto plano (sin hashear)
        usuario.PasswordHash = password;

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return (true, "Usuario creado exitosamente");
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(Usuario usuario)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            
            // Obtener el usuario actual de la BD para comparar si cambió el nombre
            var usuarioAnterior = await context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuario.Id);
            if (usuarioAnterior == null)
            {
                return (false, "Usuario no encontrado");
            }
            
            // Validar que solo puede haber un administrador (si está cambiando a ese rol)
            if (usuario.Rol == "Administrador" && usuarioAnterior.Rol != "Administrador")
            {
                var existeAdministrador = await context.Usuarios
                    .AnyAsync(u => u.Rol == "Administrador");
                
                if (existeAdministrador)
                {
                    return (false, "Ya existe un usuario con rol Administrador. Solo puede haber un administrador por sistema.");
                }
            }
            
            var nombreAnterior = usuarioAnterior.NombreUsuario?.Trim() ?? "";
            var nombreNuevo = usuario.NombreUsuario?.Trim() ?? "";
            var nombreCambio = !string.Equals(nombreAnterior, nombreNuevo, StringComparison.OrdinalIgnoreCase);
            
            Console.WriteLine($"[UsuarioService] Usuario ID: {usuario.Id}, Nombre anterior: '{nombreAnterior}', Nombre nuevo: '{nombreNuevo}', ¿Cambió?: {nombreCambio}");
            
            // Validar que no exista otro usuario con el mismo nombre
            var existeNombreUsuario = await context.Usuarios
                .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario && u.Id != usuario.Id);

            if (existeNombreUsuario)
            {
                Console.WriteLine($"[UsuarioService] ERROR: Ya existe otro usuario con el nombre '{usuario.NombreUsuario}'");
                return (false, "Ya existe otro usuario con este nombre de usuario");
            }

            // Validar que no exista otro usuario con el mismo email
            var existeEmail = await context.Usuarios
                .AnyAsync(u => u.Email == usuario.Email && u.Id != usuario.Id);

            if (existeEmail)
            {
                Console.WriteLine($"[UsuarioService] ERROR: Ya existe otro usuario con el email '{usuario.Email}'");
                return (false, "Ya existe otro usuario con este email");
            }

            // Si el nombre de usuario cambió, validar que no existan relaciones
            if (nombreCambio)
            {
                Console.WriteLine($"[UsuarioService] Validando cambio de nombre de '{nombreAnterior}' a '{nombreNuevo}'");
                
                // Verificar si existen contratos relacionados (por nombre de usuario, no por ID)
                var contratosCount = await context.Contratos
                    .Where(c => c.Comercial == nombreAnterior)
                    .CountAsync();
                
                Console.WriteLine($"[UsuarioService] Contratos encontrados con comercial '{nombreAnterior}': {contratosCount}");
                
                if (contratosCount > 0)
                {
                    return (false, $"No se puede cambiar el nombre del usuario porque tiene {contratosCount} contrato(s) asociado(s). El usuario '{nombreAnterior}' aparece como comercial en estos contratos.");
                }
                
                // Verificar si existen clientes relacionados
                var clientesCount = await context.Clientes
                    .Where(c => c.Comercial == nombreAnterior)
                    .CountAsync();
                
                Console.WriteLine($"[UsuarioService] Clientes encontrados con comercial '{nombreAnterior}': {clientesCount}");
                
                if (clientesCount > 0)
                {
                    return (false, $"No se puede cambiar el nombre del usuario porque tiene {clientesCount} cliente(s) asociado(s). El usuario '{nombreAnterior}' aparece en estos clientes.");
                }
                
                Console.WriteLine($"[UsuarioService] No se encontraron relaciones. Permitiendo cambio de nombre.");
            }

            context.Usuarios.Update(usuario);
            await context.SaveChangesAsync();

            var mensaje = nombreCambio 
                ? $"Usuario actualizado exitosamente. El nombre de usuario cambió de '{nombreAnterior}' a '{nombreNuevo}'"
                : "Usuario actualizado exitosamente";
            
            Console.WriteLine($"[UsuarioService] {mensaje}");
            return (true, mensaje);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UsuarioService] ERROR INESPERADO al actualizar usuario: {ex.Message}");
            Console.WriteLine($"[UsuarioService] StackTrace: {ex.StackTrace}");
            return (false, $"Error al actualizar el usuario: {ex.Message}");
        }
    }

    // Métodos para gestionar comercializadoras permitidas
    public async Task<List<int>> ObtenerComercializadorasPermitidasAsync(int usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.UsuarioComercializadoras
            .Where(uc => uc.UsuarioId == usuarioId)
            .Select(uc => uc.ComercializadoraId)
            .ToListAsync();
    }

    public async Task<(bool exito, string mensaje)> ActualizarComercializadorasPermitidasAsync(int usuarioId, List<int> comercializadorasIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Eliminar todas las asignaciones existentes
        var asignacionesExistentes = await context.UsuarioComercializadoras
            .Where(uc => uc.UsuarioId == usuarioId)
            .ToListAsync();
        
        context.UsuarioComercializadoras.RemoveRange(asignacionesExistentes);
        
        // Crear las nuevas asignaciones
        foreach (var comercializadoraId in comercializadorasIds)
        {
            context.UsuarioComercializadoras.Add(new UsuarioComercializadora
            {
                UsuarioId = usuarioId,
                ComercializadoraId = comercializadoraId,
                FechaAsignacion = DateTime.Now
            });
        }
        
        await context.SaveChangesAsync();
        return (true, "Comercializadoras actualizadas exitosamente");
    }

    // Métodos para gestionar operadoras permitidas
    public async Task<List<int>> ObtenerOperadorasPermitidasAsync(int usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.UsuarioOperadoras
            .Where(uo => uo.UsuarioId == usuarioId)
            .Select(uo => uo.OperadoraId)
            .ToListAsync();
    }

    public async Task<(bool exito, string mensaje)> ActualizarOperadorasPermitidasAsync(int usuarioId, List<int> operadorasIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Eliminar todas las asignaciones existentes
        var asignacionesExistentes = await context.UsuarioOperadoras
            .Where(uo => uo.UsuarioId == usuarioId)
            .ToListAsync();
        
        context.UsuarioOperadoras.RemoveRange(asignacionesExistentes);
        
        // Crear las nuevas asignaciones
        foreach (var operadoraId in operadorasIds)
        {
            context.UsuarioOperadoras.Add(new UsuarioOperadora
            {
                UsuarioId = usuarioId,
                OperadoraId = operadoraId,
                FechaAsignacion = DateTime.Now
            });
        }
        
        await context.SaveChangesAsync();
        return (true, "Operadoras actualizadas exitosamente");
    }

    // Métodos para gestionar empresas de alarmas permitidas
    public async Task<List<int>> ObtenerEmpresasAlarmasPermitidasAsync(int usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.UsuarioEmpresasAlarmas
            .Where(uea => uea.UsuarioId == usuarioId)
            .Select(uea => uea.EmpresaAlarmaId)
            .ToListAsync();
    }

    public async Task<(bool exito, string mensaje)> ActualizarEmpresasAlarmasPermitidasAsync(int usuarioId, List<int> empresasAlarmasIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Eliminar todas las asignaciones existentes
        var asignacionesExistentes = await context.UsuarioEmpresasAlarmas
            .Where(uea => uea.UsuarioId == usuarioId)
            .ToListAsync();
        
        context.UsuarioEmpresasAlarmas.RemoveRange(asignacionesExistentes);
        
        // Crear las nuevas asignaciones
        foreach (var empresaAlarmaId in empresasAlarmasIds)
        {
            context.UsuarioEmpresasAlarmas.Add(new UsuarioEmpresaAlarma
            {
                UsuarioId = usuarioId,
                EmpresaAlarmaId = empresaAlarmaId,
                FechaAsignacion = DateTime.Now
            });
        }
        
        await context.SaveChangesAsync();
        return (true, "Empresas de alarmas actualizadas exitosamente");
    }

    public async Task<Dictionary<int, decimal>> ObtenerComisionesProveedorAsync(int usuarioId, string tipoProveedor)
    {
        await using var context = _dbContextProvider.CreateDbContext();

        return await context.UsuarioComisionesProveedores
            .Where(c => c.UsuarioId == usuarioId && c.TipoProveedor == tipoProveedor)
            .ToDictionaryAsync(c => c.ProveedorId, c => c.PorcentajeComision);
    }

    public async Task ActualizarComisionesProveedorAsync(int usuarioId, string tipoProveedor, List<int> proveedoresIds, Dictionary<int, decimal> comisiones)
    {
        Console.WriteLine("[UsuarioService] ===== INICIO ActualizarComisionesProveedorAsync =====");
        Console.WriteLine($"[UsuarioService] Usuario: {usuarioId}, TipoProveedor: {tipoProveedor}");
        Console.WriteLine($"[UsuarioService] Proveedores: {string.Join(", ", proveedoresIds)}");
        Console.WriteLine($"[UsuarioService] Comisiones: {string.Join(", ", comisiones.Select(c => $"{c.Key}={c.Value}%"))}");
        
        _logger.LogInformation("[ActualizarComisionesProveedorAsync] ===== INICIO =====");
        _logger.LogInformation("[ActualizarComisionesProveedorAsync] Usuario: {UsuarioId}, TipoProveedor: {TipoProveedor}", usuarioId, tipoProveedor);
        _logger.LogInformation("[ActualizarComisionesProveedorAsync] Proveedores: {Proveedores}", string.Join(", ", proveedoresIds));
        _logger.LogInformation("[ActualizarComisionesProveedorAsync] Comisiones: {Comisiones}", string.Join(", ", comisiones.Select(c => $"{c.Key}={c.Value}%")));
        
        await using var context = _dbContextProvider.CreateDbContext();

        var existentes = await context.UsuarioComisionesProveedores
            .Where(c => c.UsuarioId == usuarioId && c.TipoProveedor == tipoProveedor)
            .ToListAsync();

        context.UsuarioComisionesProveedores.RemoveRange(existentes);

        foreach (var proveedorId in proveedoresIds.Distinct())
        {
            var porcentaje = comisiones.TryGetValue(proveedorId, out var valor) ? valor : 0m;
            porcentaje = decimal.Clamp(porcentaje, 0m, 99.99m);

            context.UsuarioComisionesProveedores.Add(new UsuarioComisionProveedor
            {
                UsuarioId = usuarioId,
                TipoProveedor = tipoProveedor,
                ProveedorId = proveedorId,
                PorcentajeComision = porcentaje,
                FechaCreacion = DateTime.Now,
                FechaActualizacion = DateTime.Now
            });
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[UsuarioService] Comisiones guardadas. Ahora actualizando contratos...");

        // Actualizar comisiones de contratos existentes
        await ActualizarComisionesContratosUsuarioAsync(usuarioId, tipoProveedor, proveedoresIds, comisiones, context);
        Console.WriteLine($"[UsuarioService] ===== FIN ActualizarComisionesProveedorAsync =====");
    }

    public async Task<(bool exito, string mensaje)> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNuevo)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var usuario = await context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        // Comparación directa sin hashear
        if (usuario.PasswordHash != passwordActual)
        {
            return (false, "La contraseña actual es incorrecta");
        }

        usuario.PasswordHash = passwordNuevo;
        await context.SaveChangesAsync();

        return (true, "Contraseña actualizada exitosamente");
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        var usuario = await context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return (false, "Usuario no encontrado");
        }

        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync();

        return (true, "Usuario eliminado exitosamente");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerificarPassword(string password, string passwordHash)
    {
        var hash = HashPassword(password);
        return hash == passwordHash;
    }

    public async Task ActualizarColaboradoresDeGestorAsync(int gestorId, List<int> colaboradorIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Obtener todos los colaboradores actuales del gestor
        var colaboradoresActuales = await context.Usuarios
            .Where(u => u.GestorId == gestorId)
            .ToListAsync();
        
        // Quitar el gestor de colaboradores que ya no están seleccionados
        foreach (var colaborador in colaboradoresActuales)
        {
            if (!colaboradorIds.Contains(colaborador.Id))
            {
                colaborador.GestorId = null;
            }
        }
        
        // Asignar el gestor a los nuevos colaboradores seleccionados
        foreach (var colaboradorId in colaboradorIds)
        {
            var colaborador = await context.Usuarios.FindAsync(colaboradorId);
            if (colaborador != null && colaborador.Rol == "Colaborador")
            {
                colaborador.GestorId = gestorId;
            }
        }
        
        await context.SaveChangesAsync();
    }

    public async Task ActualizarGestoresDeJefeVentasAsync(int jefeVentasId, List<int> gestorIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Obtener todos los gestores actuales del jefe de ventas
        var gestoresActuales = await context.Usuarios
            .Where(u => u.JefeVentasId == jefeVentasId && u.Rol == "Gestor")
            .ToListAsync();
        
        // Quitar el jefe de ventas de gestores que ya no están seleccionados
        foreach (var gestor in gestoresActuales)
        {
            if (!gestorIds.Contains(gestor.Id))
            {
                gestor.JefeVentasId = null;
            }
        }
        
        // Asignar el jefe de ventas a los nuevos gestores seleccionados
        foreach (var gestorId in gestorIds)
        {
            var gestor = await context.Usuarios.FindAsync(gestorId);
            if (gestor != null && gestor.Rol == "Gestor")
            {
                gestor.JefeVentasId = jefeVentasId;
            }
        }
        
        await context.SaveChangesAsync();
    }

    public async Task ActualizarColaboradoresDeJefeVentasAsync(int jefeVentasId, List<int> colaboradorIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Obtener todos los colaboradores actuales del jefe de ventas
        var colaboradoresActuales = await context.Usuarios
            .Where(u => u.JefeVentasId == jefeVentasId && u.Rol == "Colaborador")
            .ToListAsync();
        
        // Quitar el jefe de ventas de colaboradores que ya no están seleccionados
        foreach (var colaborador in colaboradoresActuales)
        {
            if (!colaboradorIds.Contains(colaborador.Id))
            {
                colaborador.JefeVentasId = null;
            }
        }
        
        // Asignar el jefe de ventas a los nuevos colaboradores seleccionados
        foreach (var colaboradorId in colaboradorIds)
        {
            var colaborador = await context.Usuarios.FindAsync(colaboradorId);
            if (colaborador != null && colaborador.Rol == "Colaborador")
            {
                colaborador.JefeVentasId = jefeVentasId;
            }
        }
        
        await context.SaveChangesAsync();
    }

    public async Task ActualizarJefesVentasDeDirectorAsync(int directorId, List<int> jefeIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Obtener todos los jefes de ventas actuales del director
        var jefesActuales = await context.Usuarios
            .Where(u => u.DirectorComercialId == directorId && u.Rol == "Jefe de ventas")
            .ToListAsync();
        
        // Quitar el director de jefes que ya no están seleccionados
        foreach (var jefe in jefesActuales)
        {
            if (!jefeIds.Contains(jefe.Id))
            {
                jefe.DirectorComercialId = null;
                Console.WriteLine($"[JERARQUÍA] Jefe de Ventas {jefe.Id}: DirectorComercialId cambiado de {directorId} a null");
            }
        }
        
        // Asignar el director a los nuevos jefes seleccionados
        foreach (var jefeId in jefeIds)
        {
            var jefe = await context.Usuarios.FindAsync(jefeId);
            if (jefe != null && jefe.Rol == "Jefe de ventas")
            {
                var directorAnterior = jefe.DirectorComercialId;
                jefe.DirectorComercialId = directorId;
                Console.WriteLine($"[JERARQUÍA] Jefe de Ventas {jefeId}: DirectorComercialId cambiado de {directorAnterior} a {directorId}");
            }
        }
        
        await context.SaveChangesAsync();
    }

    public async Task ActualizarGestoresDeDirectorAsync(int directorId, List<int> gestorIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Obtener todos los gestores actuales del director
        var gestoresActuales = await context.Usuarios
            .Where(u => u.DirectorComercialId == directorId && u.Rol == "Gestor")
            .ToListAsync();
        
        // Quitar el director de gestores que ya no están seleccionados
        foreach (var gestor in gestoresActuales)
        {
            if (!gestorIds.Contains(gestor.Id))
            {
                gestor.DirectorComercialId = null;
                Console.WriteLine($"[JERARQUÍA] Gestor {gestor.Id}: DirectorComercialId cambiado de {directorId} a null");
            }
        }
        
        // Asignar el director a los nuevos gestores seleccionados
        foreach (var gestorId in gestorIds)
        {
            var gestor = await context.Usuarios.FindAsync(gestorId);
            if (gestor != null && gestor.Rol == "Gestor")
            {
                var directorAnterior = gestor.DirectorComercialId;
                gestor.DirectorComercialId = directorId;
                Console.WriteLine($"[JERARQUÍA] Gestor {gestorId}: DirectorComercialId cambiado de {directorAnterior} a {directorId}");
            }
        }
        
        await context.SaveChangesAsync();
    }

    public async Task ActualizarColaboradoresDeDirectorAsync(int directorId, List<int> colaboradorIds)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Obtener todos los colaboradores actuales del director
        var colaboradoresActuales = await context.Usuarios
            .Where(u => u.DirectorComercialId == directorId && u.Rol == "Colaborador")
            .ToListAsync();
        
        // Quitar el director de colaboradores que ya no están seleccionados
        foreach (var colaborador in colaboradoresActuales)
        {
            if (!colaboradorIds.Contains(colaborador.Id))
            {
                colaborador.DirectorComercialId = null;
                Console.WriteLine($"[JERARQUÍA] Colaborador {colaborador.Id}: DirectorComercialId cambiado de {directorId} a null");
            }
        }
        
        // Asignar el director a los nuevos colaboradores seleccionados
        foreach (var colaboradorId in colaboradorIds)
        {
            var colaborador = await context.Usuarios.FindAsync(colaboradorId);
            if (colaborador != null && colaborador.Rol == "Colaborador")
            {
                var directorAnterior = colaborador.DirectorComercialId;
                colaborador.DirectorComercialId = directorId;
                Console.WriteLine($"[JERARQUÍA] Colaborador {colaboradorId}: DirectorComercialId cambiado de {directorAnterior} a {directorId}");
            }
        }
        
        await context.SaveChangesAsync();
    }
    
    // Obtener usuarios (comerciales) permitidos según el rol del usuario actual
    public async Task<List<Usuario>> ObtenerUsuariosPermitidosAsync(string? rolUsuario, int? usuarioId)
    {
        await using var context = _dbContextProvider.CreateDbContext();
        
        if (rolUsuario == "Administrador")
        {
            // Administrador ve todos los usuarios
            return await context.Usuarios
                .Where(u => u.Activo && u.Id != -1 && u.NombreUsuario != "superadmin")
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();
        }
        
        if (!usuarioId.HasValue)
        {
            return new List<Usuario>();
        }
        
        var usuariosPermitidos = new List<Usuario>();
        
        if (rolUsuario == "Colaborador")
        {
            // Colaborador solo se ve a sí mismo
            var colaborador = await context.Usuarios.FindAsync(usuarioId.Value);
            if (colaborador != null)
            {
                usuariosPermitidos.Add(colaborador);
            }
        }
        else if (rolUsuario == "Gestor")
        {
            // El gestor se ve a sí mismo y a sus colaboradores
            var gestor = await context.Usuarios.FindAsync(usuarioId.Value);
            if (gestor != null)
            {
                usuariosPermitidos.Add(gestor);
            }
            
            var colaboradores = await context.Usuarios
                .Where(u => u.GestorId == usuarioId.Value && u.Activo)
                .ToListAsync();
            usuariosPermitidos.AddRange(colaboradores);
        }
        else if (rolUsuario == "Jefe de ventas")
        {
            // El jefe de ventas ve: sí mismo, sus colaboradores directos, sus gestores y los colaboradores de esos gestores
            var jefe = await context.Usuarios.FindAsync(usuarioId.Value);
            if (jefe != null)
            {
                usuariosPermitidos.Add(jefe);
            }
            
            var colaboradoresDirectos = await context.Usuarios
                .Where(u => u.JefeVentasId == usuarioId.Value && u.Rol == "Colaborador" && u.Activo)
                .ToListAsync();
            usuariosPermitidos.AddRange(colaboradoresDirectos);
            
            var gestores = await context.Usuarios
                .Where(u => u.JefeVentasId == usuarioId.Value && u.Rol == "Gestor" && u.Activo)
                .ToListAsync();
            usuariosPermitidos.AddRange(gestores);
            
            foreach (var gestor in gestores)
            {
                var colaboradoresGestor = await context.Usuarios
                    .Where(u => u.GestorId == gestor.Id && u.Activo)
                    .ToListAsync();
                usuariosPermitidos.AddRange(colaboradoresGestor);
            }
        }
        else if (rolUsuario == "Director comercial")
        {
            // El director comercial ve toda su estructura
            var director = await context.Usuarios.FindAsync(usuarioId.Value);
            if (director != null)
            {
                usuariosPermitidos.Add(director);
            }
            
            // Jefes de ventas del director
            var jefesVentas = await context.Usuarios
                .Where(u => u.DirectorComercialId == usuarioId.Value && u.Rol == "Jefe de ventas" && u.Activo)
                .ToListAsync();
            usuariosPermitidos.AddRange(jefesVentas);
            
            foreach (var jefe in jefesVentas)
            {
                var colaboradoresJefe = await context.Usuarios
                    .Where(u => u.JefeVentasId == jefe.Id && u.Rol == "Colaborador" && u.Activo)
                    .ToListAsync();
                usuariosPermitidos.AddRange(colaboradoresJefe);
                
                var gestoresJefe = await context.Usuarios
                    .Where(u => u.JefeVentasId == jefe.Id && u.Rol == "Gestor" && u.Activo)
                    .ToListAsync();
                usuariosPermitidos.AddRange(gestoresJefe);
                
                foreach (var gestor in gestoresJefe)
                {
                    var colaboradoresGestor = await context.Usuarios
                        .Where(u => u.GestorId == gestor.Id && u.Activo)
                        .ToListAsync();
                    usuariosPermitidos.AddRange(colaboradoresGestor);
                }
            }
            
            // Gestores directos del director
            var gestoresDirectos = await context.Usuarios
                .Where(u => u.DirectorComercialId == usuarioId.Value && u.Rol == "Gestor" && u.Activo)
                .ToListAsync();
            usuariosPermitidos.AddRange(gestoresDirectos);
            
            foreach (var gestor in gestoresDirectos)
            {
                var colaboradoresGestor = await context.Usuarios
                    .Where(u => u.GestorId == gestor.Id && u.Activo)
                    .ToListAsync();
                usuariosPermitidos.AddRange(colaboradoresGestor);
            }
            
            // Colaboradores directos del director
            var colaboradoresDirectos = await context.Usuarios
                .Where(u => u.DirectorComercialId == usuarioId.Value && u.Rol == "Colaborador" && u.Activo)
                .ToListAsync();
            usuariosPermitidos.AddRange(colaboradoresDirectos);
        }
        
        return usuariosPermitidos
            .GroupBy(u => u.Id)
            .Select(g => g.First())
            .OrderBy(u => u.NombreUsuario)
            .ToList();
    }

    /// <summary>
    /// Actualiza las comisiones de los contratos existentes del usuario cuando se modifican sus porcentajes por proveedor
    /// </summary>
    private async Task ActualizarComisionesContratosUsuarioAsync(
        int usuarioId, 
        string tipoProveedor, 
        List<int> proveedoresIds, 
        Dictionary<int, decimal> comisiones,
        ApplicationDbContext context)
    {
        _logger.LogInformation("[ActualizarComisionesContratos] ===== ACTUALIZANDO COMISIONES DE CONTRATOS =====");
        _logger.LogInformation("[ActualizarComisionesContratos] Usuario ID: {UsuarioId}, Tipo Proveedor: {TipoProveedor}", usuarioId, tipoProveedor);

        // Estados en los que se deben actualizar las comisiones
        var estadosActualizables = new List<string>
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
            "Activo"
        };

        // Buscar contratos del usuario en estados actualizables
        // IMPORTANTE: Buscar por UsuarioComercializadoraId (FK), NO por nombre del comercial
        var contratos = await context.Contratos
            .Where(c => c.UsuarioComercializadoraId == usuarioId && c.Estado != null && estadosActualizables.Contains(c.Estado))
            .ToListAsync();

        _logger.LogInformation("[ActualizarComisionesContratos] Contratos encontrados: {ContratosCount}", contratos.Count);
        _logger.LogInformation("[ActualizarComisionesContratos] Proveedores a actualizar: {Proveedores}", string.Join(", ", proveedoresIds));

        int contratosActualizados = 0;

        foreach (var contrato in contratos)
        {
            _logger.LogInformation("[ActualizarComisionesContratos] Procesando contrato {ContratoId} - Tipo: {Tipo}, Estado: {Estado}", contrato.Id, contrato.Tipo, contrato.Estado);
            
            // Determinar el proveedor según el tipo de contrato
            string? nombreProveedor = null;
            int? proveedorId = null;

            if (tipoProveedor == "comercializadora" && contrato.Tipo == "energia")
            {
                nombreProveedor = contrato.EnComercializadora;
                _logger.LogInformation("[ActualizarComisionesContratos] Contrato {ContratoId} - Es Energía, Comercializadora: {Comercializadora}", contrato.Id, nombreProveedor);
                if (!string.IsNullOrWhiteSpace(nombreProveedor))
                {
                    var comercializadora = await context.Comercializadoras
                        .Where(c => c.Nombre == nombreProveedor)
                        .FirstOrDefaultAsync();
                    proveedorId = comercializadora?.Id;
                    _logger.LogInformation("[ActualizarComisionesContratos] Contrato {ContratoId} - ID Comercializadora: {ComercializadoraId}", contrato.Id, proveedorId);
                }
            }
            else if (tipoProveedor == "operadora" && contrato.Tipo == "telefonia")
            {
                nombreProveedor = contrato.OperadoraTel;
                if (!string.IsNullOrWhiteSpace(nombreProveedor))
                {
                    var operadora = await context.Operadoras
                        .Where(o => o.Nombre == nombreProveedor)
                        .FirstOrDefaultAsync();
                    proveedorId = operadora?.Id;
                }
            }
            else if (tipoProveedor == "empresa_alarma" && contrato.Tipo == "alarma")
            {
                nombreProveedor = contrato.EmpresaAlarma;
                if (!string.IsNullOrWhiteSpace(nombreProveedor))
                {
                    var empresaAlarma = await context.EmpresasAlarmas
                        .Where(e => e.Nombre == nombreProveedor)
                        .FirstOrDefaultAsync();
                    proveedorId = empresaAlarma?.Id;
                }
            }
            else
            {
                _logger.LogDebug("[ActualizarComisionesContratos] Contrato {ContratoId} - Tipo no coincide. TipoProveedor esperado: {TipoProveedorEsperado}, Tipo contrato: {TipoContrato}", contrato.Id, tipoProveedor, contrato.Tipo);
            }

            // Si no coincide el tipo de proveedor o no se pudo determinar el ID, saltar
            if (!proveedorId.HasValue)
            {
                _logger.LogDebug("[ActualizarComisionesContratos] Contrato {ContratoId} - No se pudo determinar ID del proveedor, saltando", contrato.Id);
                continue;
            }
            
            if (!proveedoresIds.Contains(proveedorId.Value))
            {
                _logger.LogDebug("[ActualizarComisionesContratos] Contrato {ContratoId} - Proveedor {ProveedorId} no está en la lista de proveedores a actualizar, saltando", contrato.Id, proveedorId.Value);
                continue;
            }

            // Obtener el nuevo porcentaje de comisión para este proveedor
            if (!comisiones.TryGetValue(proveedorId.Value, out var nuevoPorcentaje))
            {
                continue;
            }

            // Obtener la comisión base de la tarifa
            decimal comisionBase = 0;

            if (contrato.Tipo == "energia")
            {
                // Intentar obtener por ID primero
                if (contrato.EnTarifaId.HasValue)
                {
                    var tarifa = await context.TarifasEnergia.FindAsync(contrato.EnTarifaId.Value);
                    comisionBase = tarifa?.Comision ?? 0;
                }
                // Si no tiene ID pero tiene nombre de tarifa, buscar por nombre
                else if (!string.IsNullOrWhiteSpace(contrato.EnTarifa) && !string.IsNullOrWhiteSpace(contrato.EnComercializadora))
                {
                    var tarifa = await context.TarifasEnergia
                        .Where(t => t.Nombre == contrato.EnTarifa && t.Empresa == contrato.EnComercializadora)
                        .FirstOrDefaultAsync();
                    comisionBase = tarifa?.Comision ?? 0;
                    
                    _logger.LogDebug("[ActualizarComisionesContratos] Contrato {ContratoId} - Tarifa buscada por nombre: {TarifaNombre}, Comisión base: {ComisionBase}", 
                        contrato.Id, contrato.EnTarifa, comisionBase);
                }
            }
            else if (contrato.Tipo == "telefonia")
            {
                // Intentar obtener por ID primero
                if (contrato.TarifaTelId.HasValue)
                {
                    var tarifaTel = await context.TarifasTelefonia.FindAsync(contrato.TarifaTelId.Value);
                    comisionBase = tarifaTel?.ComisionNew ?? 0;
                }
                // Si no tiene ID pero tiene nombre de tarifa, buscar por nombre
                // Nota: TarifaTel contiene descripción combinada (Tarifa + Fibra + GbMovil + Movil2)
                // Por ahora mantener comisión actual si no hay ID
            }
            else if (contrato.Tipo == "alarma")
            {
                // Intentar obtener por ID primero
                if (contrato.KitAlarmaId.HasValue)
                {
                    var tarifaAlarma = await context.TarifasAlarmas.FindAsync(contrato.KitAlarmaId.Value);
                    comisionBase = tarifaAlarma?.Comision ?? 0;
                }
                // Si no tiene ID pero tiene nombre de kit, buscar por nombre
                else if (!string.IsNullOrWhiteSpace(contrato.KitAlarma) && !string.IsNullOrWhiteSpace(contrato.EmpresaAlarma))
                {
                    var tarifaAlarma = await context.TarifasAlarmas
                        .Where(t => t.NombreTarifa == contrato.KitAlarma && t.Empresa == contrato.EmpresaAlarma)
                        .FirstOrDefaultAsync();
                    comisionBase = tarifaAlarma?.Comision ?? 0;
                }
            }

            if (comisionBase == 0)
            {
                _logger.LogWarning("[ActualizarComisionesContratos] Contrato {ContratoId} - Sin comisión base, manteniendo comisión actual", contrato.Id);
                continue;
            }

            // Calcular nueva comisión: comisionBase * (porcentaje / 100)
            decimal nuevaComision = Math.Round(comisionBase * (nuevoPorcentaje / 100), 2);

            _logger.LogInformation("[ActualizarComisionesContratos] Contrato {ContratoId} ({Tipo}) - Proveedor: {Proveedor} (ID:{ProveedorId}) - Comisión: {ComisionAnterior:F2}€ → {ComisionNueva:F2}€ (Base: {ComisionBase:F2}€, %: {Porcentaje:F2})", 
                contrato.Id, contrato.Tipo, nombreProveedor, proveedorId, contrato.Comision, nuevaComision, comisionBase, nuevoPorcentaje);

            // Actualizar la comisión del contrato
            contrato.Comision = nuevaComision;
            contrato.FechaModificacion = DateTime.Now;
            context.Contratos.Update(contrato);
            contratosActualizados++;
        }

        if (contratosActualizados > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("[ActualizarComisionesContratos] ✓ {ContratosActualizados} contratos actualizados correctamente", contratosActualizados);
        }
        else
        {
            _logger.LogInformation("[ActualizarComisionesContratos] No se actualizaron contratos");
        }
    }
}
