using EnerfoneCRM.Data;
using EnerfoneCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace EnerfoneCRM.Services;

public class ConfiguracionService
{
    private readonly DbContextProvider _dbContextProvider;

    public ConfiguracionService(DbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }

    public async Task<ConfiguracionEmpresa?> ObtenerConfiguracionAsync()
    {
        await using var context = _dbContextProvider.CreateDbContext();
        return await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
    }

    public async Task<(bool exito, string mensaje)> GuardarConfiguracionAsync(ConfiguracionEmpresa configuracion)
    {
        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var existente = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
            
            if (existente != null)
            {
                // Actualizar existente
                existente.NombreEmpresa = configuracion.NombreEmpresa;
                existente.Cif = configuracion.Cif;
                existente.Direccion = configuracion.Direccion;
                existente.CodigoPostal = configuracion.CodigoPostal;
                existente.Ciudad = configuracion.Ciudad;
                existente.Provincia = configuracion.Provincia;
                existente.Pais = configuracion.Pais;
                existente.Telefono = configuracion.Telefono;
                existente.Email = configuracion.Email;
                existente.Web = configuracion.Web;
                existente.LogoUrl = configuracion.LogoUrl;
                existente.FechaActualizacion = DateTime.Now;
                existente.TemaVisual = configuracion.TemaVisual;
                existente.TemaColorAcento = configuracion.TemaColorAcento;
                existente.TemaColorBarra = configuracion.TemaColorBarra;
                existente.TemaColorFondo = configuracion.TemaColorFondo;
                existente.TemaVisualAnterior = configuracion.TemaVisualAnterior;
                existente.TemaAnteriorColorAcento = configuracion.TemaAnteriorColorAcento;
                existente.TemaAnteriorColorBarra = configuracion.TemaAnteriorColorBarra;
                existente.TemaAnteriorColorFondo = configuracion.TemaAnteriorColorFondo;
                existente.TemaActualizadoPor = configuracion.TemaActualizadoPor;
                existente.TemaActualizadoEn = configuracion.TemaActualizadoEn;
                
                // Configuración SMTP
                existente.SmtpServidor = configuracion.SmtpServidor;
                existente.SmtpPuerto = configuracion.SmtpPuerto;
                existente.SmtpUsuario = configuracion.SmtpUsuario;
                existente.SmtpPassword = configuracion.SmtpPassword;
                existente.SmtpUsarSsl = configuracion.SmtpUsarSsl;
                existente.SmtpEmailDesde = configuracion.SmtpEmailDesde;
                existente.SmtpNombreDesde = configuracion.SmtpNombreDesde;
                
                // Control de módulos
                existente.ModuloLiquidacionesActivo = configuracion.ModuloLiquidacionesActivo;
                existente.ModuloSipsActivo = configuracion.ModuloSipsActivo;
                existente.EnvioDocumentosFirmaAutomatico = configuracion.EnvioDocumentosFirmaAutomatico;
                
                context.ConfiguracionesEmpresa.Update(existente);
            }
            else
            {
                // Crear nuevo
                context.ConfiguracionesEmpresa.Add(configuracion);
            }
            
            await context.SaveChangesAsync();
            return (true, "Configuración guardada exitosamente");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar configuración: {ex.Message}");
        }
    }

    public async Task<(bool exito, string mensaje)> GuardarPaletaAsync(PaletaVisual paleta, string usuario)
    {
        if (!AparienciaService.EsPaletaValida(paleta.Id) ||
            paleta.Id == AparienciaService.PaletaPersonalizada &&
            (!AparienciaService.EsColorValido(paleta.Acento) ||
             !AparienciaService.EsColorValido(paleta.BarraLateral) ||
             !AparienciaService.EsColorValido(paleta.Fondo)))
            return (false, "La paleta seleccionada no es válida.");

        try
        {
            await using var context = _dbContextProvider.CreateDbContext();
            var configuracion = await context.ConfiguracionesEmpresa.FirstOrDefaultAsync();
            if (configuracion == null)
                return (false, "Primero debe guardarse la configuración de empresa.");

            var anterior = AparienciaService.Resolver(
                configuracion.TemaVisual,
                configuracion.TemaColorAcento,
                configuracion.TemaColorBarra,
                configuracion.TemaColorFondo);
            configuracion.TemaVisualAnterior = anterior.Id;
            configuracion.TemaAnteriorColorAcento = anterior.Acento;
            configuracion.TemaAnteriorColorBarra = anterior.BarraLateral;
            configuracion.TemaAnteriorColorFondo = anterior.Fondo;
            configuracion.TemaVisual = paleta.Id;
            if (paleta.Id == AparienciaService.PaletaPersonalizada)
            {
                configuracion.TemaColorAcento = paleta.Acento;
                configuracion.TemaColorBarra = paleta.BarraLateral;
                configuracion.TemaColorFondo = paleta.Fondo;
            }
            configuracion.TemaActualizadoPor = usuario;
            configuracion.TemaActualizadoEn = DateTime.Now;
            configuracion.FechaActualizacion = DateTime.Now;
            await context.SaveChangesAsync();
            return (true, "Apariencia aplicada a la empresa.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar la apariencia: {ex.Message}");
        }
    }
}
