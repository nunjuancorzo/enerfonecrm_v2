-- Script de diagnóstico para liquidaciones de roles adicionales

-- 1. Ver contratos vinculados a liquidaciones pendientes/aceptadas
SELECT 
    c.Id AS ContratoId,
    c.Tipo,
    c.Estado,
    c.Comercial,
    c.HistoricoLiquidacionId,
    hl.Estado AS EstadoLiquidacion,
    hl.UsuarioNombre
FROM Contratos c
LEFT JOIN HistoricoLiquidaciones hl ON c.HistoricoLiquidacionId = hl.Id
WHERE c.HistoricoLiquidacionId IS NOT NULL
ORDER BY c.HistoricoLiquidacionId DESC
LIMIT 20;

-- 2. Ver decomisiones de esos contratos
SELECT 
    d.Id,
    d.ContratoId,
    d.UsuarioId,
    u.NombreUsuario,
    u.Rol,
    d.ImporteDecomision,
    d.Comision,
    c.Comercial AS DueñoContrato
FROM Decomisiones d
INNER JOIN Usuarios u ON d.UsuarioId = u.Id
INNER JOIN Contratos c ON d.ContratoId = c.Id
WHERE d.ContratoId IN (
    SELECT Id FROM Contratos 
    WHERE HistoricoLiquidacionId IS NOT NULL
)
ORDER BY d.ContratoId DESC
LIMIT 30;

-- 3. Ver todas las liquidaciones recientes
SELECT 
    Id,
    UsuarioId,
    UsuarioNombre,
    Estado,
    CantidadContratos,
    TotalComisiones,
    FechaAprobacion,
    FechaAceptada
FROM HistoricoLiquidaciones
ORDER BY Id DESC
LIMIT 10;

-- 4. Ver usuarios activos y sus roles
SELECT 
    Id,
    NombreUsuario,
    Rol,
    Activo
FROM Usuarios
WHERE Activo = 1
ORDER BY Rol, NombreUsuario;
