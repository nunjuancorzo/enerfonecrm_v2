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
        // Agregar COALESCE para campos TEXT problemáticos
        if (command.CommandText.Contains("contratos"))
        {
            command.CommandText = command.CommandText
                .Replace("c.direccion", "COALESCE(c.direccion, '')")
                .Replace("c.observaciones", "COALESCE(c.observaciones, '')")
                .Replace("c.en_Tarifa", "COALESCE(c.en_Tarifa, '')")
                .Replace("c.en_CUPS", "COALESCE(c.en_CUPS, '')")
                .Replace("c.en_Servicios", "COALESCE(c.en_Servicios, '')")
                .Replace("c.en_IBAN", "COALESCE(c.en_IBAN, '')")
                .Replace("c.en_Titular", "COALESCE(c.en_Titular, '')")
                .Replace("c.en_DNI", "COALESCE(c.en_DNI, '')")
                .Replace("c.observaciones_alarma", "COALESCE(c.observaciones_alarma, '')")
                .Replace("c.observaciones_estado", "COALESCE(c.observaciones_estado, '')");
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
        if (command.CommandText.Contains("contratos"))
        {
            command.CommandText = command.CommandText
                .Replace("c.direccion", "COALESCE(c.direccion, '')")
                .Replace("c.observaciones", "COALESCE(c.observaciones, '')")
                .Replace("c.en_Tarifa", "COALESCE(c.en_Tarifa, '')")
                .Replace("c.en_CUPS", "COALESCE(c.en_CUPS, '')")
                .Replace("c.en_Servicios", "COALESCE(c.en_Servicios, '')")
                .Replace("c.en_IBAN", "COALESCE(c.en_IBAN, '')")
                .Replace("c.en_Titular", "COALESCE(c.en_Titular, '')")
                .Replace("c.en_DNI", "COALESCE(c.en_DNI, '')")
                .Replace("c.observaciones_alarma", "COALESCE(c.observaciones_alarma, '')")
                .Replace("c.observaciones_estado", "COALESCE(c.observaciones_estado, '')");
        }
        
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }
}
