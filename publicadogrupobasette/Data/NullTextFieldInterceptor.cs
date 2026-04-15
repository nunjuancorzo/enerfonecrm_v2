using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace EnerfoneCRM.Data;

public class NullTextFieldInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        // Agregar COALESCE para campos TEXT problemáticos en contratos
        if (command.CommandText.Contains("contratos") || command.CommandText.Contains("FROM `contratos`"))
        {
            // Lista completa de campos de texto que pueden ser NULL
            var camposTexto = new[]
            {
                "tipo", "estado", "comercial", "nombre_cliente", "dni", "direccion", "iban",
                "estadoServicio", "en_Comercializadora", "en_Tarifa", "en_CUPS", "en_CUPSGas",
                "en_Servicios", "en_IBAN", "tipoOperacion", "operadora_tel", "Tarifa_tel",
                "TipoTarifa_tel", "fijo_tel", "LineaMovilPrincipal", "tipo_linea_movil_principal",
                "codigo_icc_principal", "telefono_linea1_tel", "tarifa_linea1_tel", "tipo_linea1_tel",
                "codigo_icc_linea1_tel", "telefono_linea2_tel", "tarifa_linea2_tel", "tipo_linea2_tel",
                "codigo_icc_linea2_tel", "telefono_linea3_tel", "tarifa_linea3_tel", "tipo_linea3_tel",
                "codigo_icc_linea3_tel", "telefono_linea4_tel", "tarifa_linea4_tel", "tipo_linea4_tel",
                "codigo_icc_linea4_tel", "telefono_linea5_tel", "tarifa_linea5_tel", "tipo_linea5_tel",
                "codigo_icc_linea5_tel", "horario_instalacion_tel", "contratar", "TV",
                "tipo_alarma", "subtipo_inmueble", "compania_anterior", "numero_contrato_anterior",
                "kit_alarma", "opcionales_alarma", "campana_alarma", "empresa_alarma",
                "direccion_instalacion_alarma", "numero_instalacion", "escalera_instalacion",
                "piso_instalacion", "puerta_instalacion", "codigo_postal_instalacion",
                "provincia_instalacion", "localidad_instalacion", "aclarador_instalacion",
                "observaciones_alarma", "observaciones_estado", "titular_iban_dni",
                "titular_iban_nombre", "titular_iban_numero", "pdf_contrato_url"
            };

            foreach (var campo in camposTexto)
            {
                command.CommandText = command.CommandText
                    .Replace($"c.{campo}", $"COALESCE(c.{campo}, '')")
                    .Replace($"`c`.`{campo}`", $"COALESCE(`c`.`{campo}`, '')")
                    .Replace($"contratos.{campo}", $"COALESCE(contratos.{campo}, '')");
            }
        }
        
        if (command.CommandText.Contains("log_activaciones_contratos"))
        {
            command.CommandText = command.CommandText
                .Replace("l.usuario", "COALESCE(l.usuario, '')")
                .Replace("l.observaciones", "COALESCE(l.observaciones, '')")
                .Replace("`l`.`usuario`", "COALESCE(`l`.`usuario`, '')")
                .Replace("`l`.`observaciones`", "COALESCE(`l`.`observaciones`, '')");
        }
        
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        // Mismo procesamiento para async
        if (command.CommandText.Contains("contratos") || command.CommandText.Contains("FROM `contratos`"))
        {
            // Lista completa de campos de texto que pueden ser NULL
            var camposTexto = new[]
            {
                "tipo", "estado", "comercial", "nombre_cliente", "dni", "direccion", "iban",
                "estadoServicio", "en_Comercializadora", "en_Tarifa", "en_CUPS", "en_CUPSGas",
                "en_Servicios", "en_IBAN", "tipoOperacion", "operadora_tel", "Tarifa_tel",
                "TipoTarifa_tel", "fijo_tel", "LineaMovilPrincipal", "tipo_linea_movil_principal",
                "codigo_icc_principal", "telefono_linea1_tel", "tarifa_linea1_tel", "tipo_linea1_tel",
                "codigo_icc_linea1_tel", "telefono_linea2_tel", "tarifa_linea2_tel", "tipo_linea2_tel",
                "codigo_icc_linea2_tel", "telefono_linea3_tel", "tarifa_linea3_tel", "tipo_linea3_tel",
                "codigo_icc_linea3_tel", "telefono_linea4_tel", "tarifa_linea4_tel", "tipo_linea4_tel",
                "codigo_icc_linea4_tel", "telefono_linea5_tel", "tarifa_linea5_tel", "tipo_linea5_tel",
                "codigo_icc_linea5_tel", "horario_instalacion_tel", "contratar", "TV",
                "tipo_alarma", "subtipo_inmueble", "compania_anterior", "numero_contrato_anterior",
                "kit_alarma", "opcionales_alarma", "campana_alarma", "empresa_alarma",
                "direccion_instalacion_alarma", "numero_instalacion", "escalera_instalacion",
                "piso_instalacion", "puerta_instalacion", "codigo_postal_instalacion",
                "provincia_instalacion", "localidad_instalacion", "aclarador_instalacion",
                "observaciones_alarma", "observaciones_estado", "titular_iban_dni",
                "titular_iban_nombre", "titular_iban_numero", "pdf_contrato_url"
            };

            foreach (var campo in camposTexto)
            {
                command.CommandText = command.CommandText
                    .Replace($"c.{campo}", $"COALESCE(c.{campo}, '')")
                    .Replace($"`c`.`{campo}`", $"COALESCE(`c`.`{campo}`, '')")
                    .Replace($"contratos.{campo}", $"COALESCE(contratos.{campo}, '')");
            }
        }
        
        if (command.CommandText.Contains("log_activaciones_contratos"))
        {
            command.CommandText = command.CommandText
                .Replace("l.usuario", "COALESCE(l.usuario, '')")
                .Replace("l.observaciones", "COALESCE(l.observaciones, '')")
                .Replace("`l`.`usuario`", "COALESCE(`l`.`usuario`, '')")
                .Replace("`l`.`observaciones`", "COALESCE(`l`.`observaciones`, '')");
        }
        
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
