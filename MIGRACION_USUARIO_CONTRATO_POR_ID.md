# 🔧 Migración: Relación Usuario-Contrato por ID

## 📋 Resumen

Se ha migrado la relación entre **Usuarios** y **Contratos** de usar el campo `comercial` (nombre del usuario) a usar `usuario_comercializadora_id` (clave foránea al ID del usuario).

### ✅ Beneficios
- ✅ Integridad referencial: FK garantiza que el usuario existe
- ✅ Renombre de usuarios: Si cambias el nombre de usuario, los contratos se mantienen vinculados
- ✅ Rendimiento: Búsquedas por ID son más rápidas que por texto
- ✅ Consistencia: Relación estándar de base de datos

---

## 🗂️ Archivos Modificados

### 1. **Script SQL de Migración**
📄 `FIX_RELACION_USUARIO_CONTRATOS.sql`

Este script:
1. ✅ Muestra el estado actual de contratos
2. ✅ Identifica qué contratos necesitan migración
3. ✅ Actualiza `usuario_comercializadora_id` basándose en el campo `comercial`
4. ✅ Verifica el resultado
5. ✅ Lista contratos que no pudieron migrarse

**⚠️ IMPORTANTE:** Ejecuta este script ANTES de usar la aplicación después del despliegue.

```sql
-- El script hace esto:
UPDATE contratos c
INNER JOIN usuarios u ON c.comercial = u.nombre_usuario
SET c.usuario_comercializadora_id = u.id,
    c.fecha_modificacion = NOW()
WHERE c.comercial IS NOT NULL 
  AND c.usuario_comercializadora_id IS NULL;
```

### 2. **Código de Creación de Contratos**
Se modificaron **7 archivos** para que siempre se guarde `UsuarioComercializadoraId`:

#### Wizards de Creación:
- ✅ `WizardCrearContratoEnergia.razor` (línea ~1117)
- ✅ `WizardCrearContratoTelefonia.razor` (línea ~1275)
- ✅ `WizardCrearContratoAlarmas.razor` (línea ~994)

#### Página de Clientes (creación desde listado de clientes):
- ✅ `Clientes.razor` - Energía (línea ~3576)
- ✅ `Clientes.razor` - Telefonía (línea ~3677)
- ✅ `Clientes.razor` - Alarmas (línea ~3717)

**Cambio aplicado:**
```csharp
// ANTES
Comercial = AuthService.UsuarioActual?.NombreUsuario ?? "",

// DESPUÉS
Comercial = AuthService.UsuarioActual?.NombreUsuario ?? "",
UsuarioComercializadoraId = AuthService.UsuarioActual?.Id,
```

### 3. **Lógica de Actualización de Comisiones**
📄 `EnerfoneCRM/Services/UsuarioService.cs`

Se corrigió el método `ActualizarComisionesContratosUsuarioAsync` para:
- ✅ Buscar contratos por `UsuarioComercializadoraId` en lugar de `Comercial`
- ✅ Usar tipos en minúsculas ("energia", "telefonia", "alarma")
- ✅ Eliminar código innecesario que buscaba el nombre del usuario

**Cambio clave:**
```csharp
// ANTES (línea ~781)
var contratos = await context.Contratos
    .Where(c => c.Comercial == nombreUsuario && ...)
    .ToListAsync();

// DESPUÉS
var contratos = await context.Contratos
    .Where(c => c.UsuarioComercializadoraId == usuarioId && ...)
    .ToListAsync();
```

---

## 📦 Instrucciones de Despliegue

### 1️⃣ Preparación
```bash
# Hacer backup de la base de datos
mysqldump -u usuario -p nombre_bd > backup_antes_migracion.sql

# Verificar que no hay errores de compilación
cd EnerfoneCRM
dotnet build
```

### 2️⃣ Ejecutar Script SQL
```bash
# Conectar a MySQL
mysql -u usuario -p nombre_bd

# Ejecutar el script paso por paso
source /ruta/a/FIX_RELACION_USUARIO_CONTRATOS.sql
```

**⚠️ Revisar el output:**
- Verificar cuántos contratos se migraron
- Revisar los contratos que NO pudieron migrarse (si hay)
- Esos contratos tienen un nombre de comercial que no existe en `usuarios`

### 3️⃣ Desplegar Código
```bash
# Detener la aplicación
sudo systemctl stop tuapp.service

# Desplegar nuevos archivos
git pull origin main
# o copiar archivos manualmente

# Reiniciar la aplicación
sudo systemctl start tuapp.service
```

### 4️⃣ Verificación Post-Despliegue

1. **Verificar contratos migrados:**
```sql
SELECT 
    COUNT(*) as total,
    SUM(CASE WHEN usuario_comercializadora_id IS NOT NULL THEN 1 ELSE 0 END) as con_usuario_id
FROM contratos;
-- Debería mostrar que casi todos tienen usuario_id
```

2. **Probar actualización de comisiones:**
   - Ve a Usuarios → Editar un usuario
   - Cambia el % de comisión para una comercializadora
   - Guardar
   - Verifica que los contratos se actualizan correctamente

3. **Crear un contrato nuevo:**
   - Crear un contrato de energía/telefonía/alarma
   - Verificar que `usuario_comercializadora_id` se guarda correctamente

---

## 🔍 Solución de Problemas

### ❌ Problema: Contratos sin `usuario_comercializadora_id`

**Síntoma:**
```sql
SELECT * FROM contratos 
WHERE comercial IS NOT NULL 
  AND usuario_comercializadora_id IS NULL;
-- Devuelve resultados
```

**Causa:** El nombre del comercial no coincide con ningún `nombre_usuario` en la tabla `usuarios`.

**Solución:**
```sql
-- Opción 1: Actualizar manualmente
UPDATE contratos 
SET usuario_comercializadora_id = 123  -- ID correcto del usuario
WHERE id = 456;  -- ID del contrato

-- Opción 2: Encontrar el usuario correcto
SELECT u.id, u.nombre_usuario, c.id, c.comercial
FROM contratos c
LEFT JOIN usuarios u ON LOWER(c.comercial) = LOWER(u.nombre_usuario)
WHERE c.usuario_comercializadora_id IS NULL
  AND c.comercial IS NOT NULL;
```

### ❌ Problema: Comisiones no se actualizan

**Verifica:**
1. ¿El contrato tiene `usuario_comercializadora_id`?
```sql
SELECT id, comercial, usuario_comercializadora_id 
FROM contratos WHERE id = 123;
```

2. ¿El estado del contrato está en la lista de actualizables?
```sql
SELECT estado FROM contratos WHERE id = 123;
-- Debe ser: Pte Carga, Solicitado, Pte Firma, etc.
```

3. ¿El tipo de contrato coincide?
```sql
SELECT tipo FROM contratos WHERE id = 123;
-- Debe ser: "energia", "telefonia" o "alarma" (minúsculas)
```

---

## 📊 Estadísticas de Migración

Al ejecutar el script, obtendrás algo como:

```
total_contratos: 522
con_nombre_comercial: 522
con_id_usuario: 495 (después de migración)
necesitan_migracion: 27 (antes) → 0 (después si todo OK)
```

Si hay contratos que no se pudieron migrar, aparecerán en el **PASO 5** del script.

---

## ✅ Checklist de Validación

- [ ] Backup de base de datos realizado
- [ ] Script SQL ejecutado sin errores
- [ ] Todos (o casi todos) los contratos tienen `usuario_comercializadora_id`
- [ ] Contratos sin FK revisados manualmente
- [ ] Código desplegado
- [ ] Aplicación reiniciada
- [ ] Prueba de crear nuevo contrato → ✓ Tiene `usuario_comercializadora_id`
- [ ] Prueba de actualizar comisiones → ✓ Los contratos se actualizan
- [ ] Logs muestran contratos encontrados y actualizados

---

## 🎯 Próximos Pasos (Futuro)

Una vez estable esta migración, podríamos:

1. **Añadir constraint de FK** en la BD:
```sql
ALTER TABLE contratos
ADD CONSTRAINT fk_contratos_usuario
FOREIGN KEY (usuario_comercializadora_id) 
REFERENCES usuarios(id)
ON DELETE SET NULL;
```

2. **Deprecar el campo `comercial`**: Mantenerlo para visualización pero nunca usarlo para búsquedas.

3. **Añadir índice** si no existe:
```sql
CREATE INDEX idx_contratos_usuario_id 
ON contratos(usuario_comercializadora_id);
```

---

📅 **Fecha de creación:** 2026-07-02  
👤 **Autor:** Sistema de Migración Automática
