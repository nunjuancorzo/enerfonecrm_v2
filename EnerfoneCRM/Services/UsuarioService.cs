using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EnerfoneCRM.Services;

public class UsuarioService
{
    private readonly DbContextProvider _dbContextProvider;

    public UsuarioService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
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
        await using var context = _dbContextProvider.CreateDbContext();
        
        // Obtener el usuario actual de la BD para comparar si cambió el nombre
        var usuarioAnterior = await context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuario.Id);
        if (usuarioAnterior == null)
        {
            return (false, "Usuario no encontrado");
        }
        
        var nombreAnterior = usuarioAnterior.NombreUsuario;
        var nombreNuevo = usuario.NombreUsuario;
        var nombreCambio = nombreAnterior != nombreNuevo;
        
        var existeNombreUsuario = await context.Usuarios
            .AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario && u.Id != usuario.Id);

        if (existeNombreUsuario)
        {
            return (false, "Ya existe otro usuario con este nombre de usuario");
        }

        var existeEmail = await context.Usuarios
            .AnyAsync(u => u.Email == usuario.Email && u.Id != usuario.Id);

        if (existeEmail)
        {
            return (false, "Ya existe otro usuario con este email");
        }

        // Si el nombre de usuario cambió, actualizar todas las referencias
        if (nombreCambio)
        {
            Console.WriteLine($"[UsuarioService] Actualizando referencias de '{nombreAnterior}' a '{nombreNuevo}'");
            
            // 1. Actualizar Contratos.Comercial
            var contratos = await context.Contratos
                .Where(c => c.Comercial == nombreAnterior)
                .ToListAsync();
            
            foreach (var contrato in contratos)
            {
                contrato.Comercial = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizados {contratos.Count} contratos");
            
            // 2. Actualizar Clientes.NombreUsuario
            var clientes = await context.Clientes
                .Where(c => c.NombreUsuario == nombreAnterior)
                .ToListAsync();
            
            foreach (var cliente in clientes)
            {
                cliente.NombreUsuario = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizados {clientes.Count} clientes");
            
            // 3. Actualizar TareaPendiente (ambos campos)
            var tareasAsignado = await context.TareasPendientes
                .Where(t => t.NombreUsuarioAsignado == nombreAnterior)
                .ToListAsync();
            
            foreach (var tarea in tareasAsignado)
            {
                tarea.NombreUsuarioAsignado = nombreNuevo;
            }
            
            var tareasCreador = await context.TareasPendientes
                .Where(t => t.NombreUsuarioCreador == nombreAnterior)
                .ToListAsync();
            
            foreach (var tarea in tareasCreador)
            {
                tarea.NombreUsuarioCreador = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizadas {tareasAsignado.Count + tareasCreador.Count} tareas pendientes");
            
            // 4. Actualizar Incidencia.NombreUsuario
            var incidencias = await context.Incidencias
                .Where(i => i.NombreUsuario == nombreAnterior)
                .ToListAsync();
            
            foreach (var incidencia in incidencias)
            {
                incidencia.NombreUsuario = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizadas {incidencias.Count} incidencias");
            
            // 5. Actualizar HistoricoLiquidacion (ambos campos)
            var liquidacionesUsuario = await context.HistoricoLiquidaciones
                .Where(h => h.UsuarioNombre == nombreAnterior)
                .ToListAsync();
            
            foreach (var liquidacion in liquidacionesUsuario)
            {
                liquidacion.UsuarioNombre = nombreNuevo;
            }
            
            var liquidacionesAprobador = await context.HistoricoLiquidaciones
                .Where(h => h.AprobadoPorNombre == nombreAnterior)
                .ToListAsync();
            
            foreach (var liquidacion in liquidacionesAprobador)
            {
                liquidacion.AprobadoPorNombre = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizadas {liquidacionesUsuario.Count + liquidacionesAprobador.Count} liquidaciones");
            
            // 6. Actualizar LogAcceso.NombreUsuario
            var logAccesos = await context.LogAccesos
                .Where(l => l.NombreUsuario == nombreAnterior)
                .ToListAsync();
            
            foreach (var log in logAccesos)
            {
                log.NombreUsuario = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizados {logAccesos.Count} registros de acceso");
            
            // 7. Actualizar ObservacionContrato.Usuario
            var observaciones = await context.ObservacionesContratos
                .Where(o => o.Usuario == nombreAnterior)
                .ToListAsync();
            
            foreach (var observacion in observaciones)
            {
                observacion.Usuario = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizadas {observaciones.Count} observaciones de contratos");
            
            // 8. Actualizar LogActivacionContrato.Usuario
            var logActivaciones = await context.LogActivacionesContratos
                .Where(l => l.Usuario == nombreAnterior)
                .ToListAsync();
            
            foreach (var log in logActivaciones)
            {
                log.Usuario = nombreNuevo;
            }
            Console.WriteLine($"[UsuarioService] Actualizados {logActivaciones.Count} logs de activación");
        }

        context.Usuarios.Update(usuario);
        await context.SaveChangesAsync();

        var mensaje = nombreCambio 
            ? $"Usuario actualizado exitosamente. Se actualizaron todas las referencias de '{nombreAnterior}' a '{nombreNuevo}'"
            : "Usuario actualizado exitosamente";
        
        return (true, mensaje);
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
}
