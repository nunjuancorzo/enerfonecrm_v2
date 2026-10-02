# ANÁLISIS Y PROMPT PARA MEJORA DE JERARQUÍA DE USUARIOS

## 📊 SITUACIÓN ACTUAL

### Modelo de Datos (Usuario.cs)
```csharp
public class Usuario
{
    public int? GestorId { get; set; }           // Un colaborador puede tener un gestor
    public int? JefeVentasId { get; set; }        // Un colaborador/gestor puede tener un jefe de ventas
    public int? DirectorComercialId { get; set; } // Un colaborador/gestor/jefe puede tener un director
    public string Rol { get; set; }               // Administrador, Backoffice, Director comercial, 
                                                  // Jefe de ventas, Gestor, Colaborador, Comercializadora
}
```

### Interfaz Actual (ModalEditarUsuario.razor)

**Para COLABORADORES:**
- Dropdown para seleccionar `GestorId`
- Dropdown para seleccionar `JefeVentasId`
- Dropdown para seleccionar `DirectorComercialId`

**Para GESTORES:**
- Dropdown para seleccionar `JefeVentasId`
- Dropdown para seleccionar `DirectorComercialId`
- Checkboxes para seleccionar colaboradores que reportan a este gestor

**Para JEFES DE VENTAS:**
- Dropdown para seleccionar `DirectorComercialId`
- Checkboxes para seleccionar gestores que reportan a este jefe
- Checkboxes para seleccionar colaboradores directos que reportan a este jefe

**Para DIRECTORES COMERCIALES:**
- No hay interfaz para asignar subordinados directamente

### Cómo Funciona la Distribución de Comisiones (ComisionService.cs)

Cuando se crea un contrato con un colaborador:
1. Se lee `colaborador.GestorId` → Si existe, el gestor recibe su %
2. Se lee `colaborador.JefeVentasId` → Si existe, el jefe de ventas recibe su %
3. Se lee `colaborador.DirectorComercialId` → Si existe, el director recibe su %
4. El resto va al Administrador

**Ejemplo:**
- Contrato de 100€ de comisión base
- Colaborador: 70% → 70€
- Gestor: 10% → 10€ (del GestorId del colaborador)
- Jefe Ventas: 10% → 10€ (del JefeVentasId del colaborador)
- Director: 5% → 5€ (del DirectorComercialId del colaborador)
- Administrador: 5% → 5€

---

## 🚨 PROBLEMAS IDENTIFICADOS

### 1. **DOBLE ASIGNACIÓN INCONSISTENTE**
- **Problema:** Los colaboradores tienen campos `GestorId`/`JefeVentasId` que se asignan mediante dropdowns, PERO al mismo tiempo los Gestores/Jefes tienen checkboxes para seleccionar a sus subordinados.
- **Riesgo:** Un colaborador puede tener `GestorId = 5`, pero el Gestor con ID=5 no tiene marcado a ese colaborador en sus checkboxes. ¿Cuál es la verdad?
- **Impacto:** Confusión sobre quién reporta a quién. Las comisiones se calculan por los campos `*Id`, pero la UI muestra otra cosa.

### 2. **FALTA DE VISTA BIDIRECCIONAL**
- **Problema:** Los Directores Comerciales NO tienen una interfaz para ver/gestionar a sus subordinados.
- **Riesgo:** Un director no puede ver fácilmente qué Jefes de Ventas/Gestores/Colaboradores están bajo su supervisión.

### 3. **JERARQUÍA COMPLEJA Y CONFUSA**
- **Problema:** Un colaborador puede tener:
  - Solo Gestor
  - Solo Jefe de Ventas
  - Gestor + Jefe de Ventas
  - Gestor + Jefe de Ventas + Director
  - Cualquier combinación posible
- **Riesgo:** No está claro cuál es la "cadena de mando" correcta. ¿Un colaborador puede saltarse el gestor y reportar directamente al jefe?

### 4. **NO HAY VALIDACIÓN DE CICLOS**
- **Problema:** Nada impide que:
  - Colaborador A tenga como Gestor a B
  - Gestor B tenga como Jefe de Ventas a C
  - Jefe C tenga como Director a A (ciclo)

### 5. **CARGA Y RECARGA REDUNDANTE**
- **Problema:** Hay múltiples funciones que cargan y recargan listas:
  - `CargarGestores()`
  - `CargarJefesVentas()`
  - `CargarDirectoresComerciales()`
  - `CargarColaboradores()`
  - Callbacks: `OnGestorChanged`, `OnJefeVentasChanged`, `OnDirectorComercialChanged`
- **Riesgo:** Performance degradada, código difícil de mantener.

### 6. **USUARIOS YA CONFIGURADOS**
- **Restricción:** Hay usuarios en producción con jerarquías ya asignadas que DEBEN mantenerse funcionando después de cualquier cambio.

---

## 💡 PROPUESTA DE MEJORA

### Objetivos
1. ✅ **Simplificar** la asignación de jerarquías
2. ✅ **Unificar** la fuente de verdad (evitar doble asignación)
3. ✅ **Clarificar** la cadena de mando
4. ✅ **Mantener compatibilidad** con usuarios existentes
5. ✅ **Mejorar UX** con vista bidireccional clara

### Solución Propuesta: **ASIGNACIÓN UNIDIRECCIONAL CON VISTA DE EQUIPO**

#### OPCIÓN A: "Asignar desde arriba" (Recomendada)

**Cambio conceptual:**
- Los subordinados NO seleccionan a sus superiores
- Los superiores SELECCIONAN a sus subordinados
- La fuente de verdad está en los campos `*Id` del subordinado
- La UI solo muestra/edita desde la perspectiva del superior

**Interfaz propuesta:**

**Para COLABORADORES:**
```
┌─────────────────────────────────────────┐
│ INFORMACIÓN DEL COLABORADOR             │
├─────────────────────────────────────────┤
│ Nombre: AgustinMendoza                  │
│ Rol: Colaborador                        │
│                                         │
│ ⚠️ JERARQUÍA (Solo lectura)            │
│ ├─ Gestor: JuanGestor                  │
│ ├─ Jefe de Ventas: MariaJefa           │
│ └─ Director Comercial: CarlosDirector  │
│                                         │
│ ℹ️ Para cambiar la jerarquía, edita    │
│    el usuario superior correspondiente  │
└─────────────────────────────────────────┘
```

**Para GESTORES:**
```
┌─────────────────────────────────────────┐
│ INFORMACIÓN DEL GESTOR                  │
├─────────────────────────────────────────┤
│ Nombre: JuanGestor                      │
│ Rol: Gestor                             │
│                                         │
│ ⚠️ REPORTA A (Solo lectura)            │
│ ├─ Jefe de Ventas: MariaJefa           │
│ └─ Director Comercial: CarlosDirector  │
│                                         │
│ 👥 EQUIPO ASIGNADO                      │
│ ┌─────────────────────────────────────┐ │
│ │ ☑ AgustinMendoza (Colaborador)     │ │
│ │ ☑ PedroColaborador (Colaborador)   │ │
│ │ ☐ LuisaColaboradora (Colaborador)  │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ Al marcar/desmarcar, se actualiza el   │
│ campo GestorId de cada colaborador     │
└─────────────────────────────────────────┘
```

**Para JEFES DE VENTAS:**
```
┌─────────────────────────────────────────┐
│ INFORMACIÓN DEL JEFE DE VENTAS          │
├─────────────────────────────────────────┤
│ Nombre: MariaJefa                       │
│ Rol: Jefe de ventas                     │
│                                         │
│ ⚠️ REPORTA A (Solo lectura)            │
│ └─ Director Comercial: CarlosDirector  │
│                                         │
│ 👥 GESTORES ASIGNADOS                   │
│ ┌─────────────────────────────────────┐ │
│ │ ☑ JuanGestor (Gestor)               │ │
│ │   └─ 2 colaboradores                │ │
│ │ ☐ AnaGestora (Gestora)              │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ 👥 COLABORADORES DIRECTOS               │
│ ┌─────────────────────────────────────┐ │
│ │ ☑ CarmenColaboradora (Colaborador)  │ │
│ │ ☐ RobertoColaborador (Colaborador)  │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ Al marcar/desmarcar, se actualiza el   │
│ campo JefeVentasId correspondiente     │
└─────────────────────────────────────────┘
```

**Para DIRECTORES COMERCIALES:**
```
┌─────────────────────────────────────────┐
│ INFORMACIÓN DEL DIRECTOR COMERCIAL      │
├─────────────────────────────────────────┤
│ Nombre: CarlosDirector                  │
│ Rol: Director comercial                 │
│                                         │
│ 📊 ESTRUCTURA DE EQUIPO                 │
│                                         │
│ 👥 JEFES DE VENTAS ASIGNADOS            │
│ ┌─────────────────────────────────────┐ │
│ │ ☑ MariaJefa (Jefe de ventas)        │ │
│ │   ├─ 1 gestor                       │ │
│ │   └─ 3 colaboradores                │ │
│ │ ☐ PabloJefe (Jefe de ventas)        │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ 👥 GESTORES DIRECTOS                    │
│ ┌─────────────────────────────────────┐ │
│ │ ☑ SofiaGestora (Gestora)            │ │
│ │   └─ 1 colaborador                  │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ 👥 COLABORADORES DIRECTOS               │
│ ┌─────────────────────────────────────┐ │
│ │ ☑ DiegoColaborador (Colaborador)    │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ Al marcar/desmarcar, se actualiza el   │
│ campo DirectorComercialId               │
└─────────────────────────────────────────┘
```

#### OPCIÓN B: "Árbol jerárquico visual"

Crear una página dedicada `/jerarquia` con un árbol visual interactivo:

```
📊 ESTRUCTURA ORGANIZATIVA

CarlosDirector (Director Comercial)
├─ MariaJefa (Jefe de Ventas)
│  ├─ JuanGestor (Gestor)
│  │  ├─ AgustinMendoza (Colaborador) [70%]
│  │  └─ PedroColaborador (Colaborador) [65%]
│  └─ CarmenColaboradora (Colaborador) [70%]
└─ SofiaGestora (Gestora)
   └─ DiegoColaborador (Colaborador) [75%]

🔄 Drag & Drop para reorganizar
⚙️ Click para editar configuración de comisiones
```

---

## 🎯 PROMPT PARA IMPLEMENTACIÓN

```
OBJETIVO: Rediseñar la gestión de jerarquías de usuarios en el CRM para hacerla más clara, intuitiva y consistente.

CONTEXTO TÉCNICO:
- Base de datos: MySQL con campos GestorId, JefeVentasId, DirectorComercialId en tabla usuarios
- Framework: Blazor Server con .NET 8
- Archivo principal: /Components/Pages/Usuarios/ModalEditarUsuario.razor
- Modelo: /Models/Usuario.cs
- Distribución de comisiones: /Services/ComisionService.cs (usa los campos *Id para calcular)

RESTRICCIONES CRÍTICAS:
1. ✅ MANTENER COMPATIBILIDAD con usuarios ya configurados (no romper datos existentes)
2. ✅ NO CAMBIAR la lógica de cálculo de comisiones en ComisionService.cs
3. ✅ La fuente de verdad sigue siendo los campos GestorId/JefeVentasId/DirectorComercialId

REQUERIMIENTOS:

1. **UNIFICAR ASIGNACIÓN:**
   - Eliminar dropdowns de selección de superiores para Colaboradores/Gestores/Jefes
   - Mantener SOLO checkboxes en la perspectiva del superior
   - Cuando un superior marca/desmarca un subordinado, se actualiza el campo *Id del subordinado

2. **VISTA DE JERARQUÍA (Solo lectura para subordinados):**
   - Colaboradores ven a quién reportan (gestor, jefe, director) pero NO pueden cambiarlo
   - Mostrar mensaje: "Para cambiar tu jerarquía, contacta a tu superior o administrador"

3. **INTERFAZ CLARA PARA SUPERIORES:**
   - Gestor: Ve y selecciona sus colaboradores (actualiza GestorId)
   - Jefe de Ventas: Ve y selecciona gestores + colaboradores directos (actualiza JefeVentasId)
   - Director Comercial: Ve y selecciona jefes + gestores directos + colaboradores directos (actualiza DirectorComercialId)

4. **MOSTRAR CONTEXTO:**
   - Al lado de cada subordinado seleccionable, mostrar su jerarquía actual
   - Ejemplo: "AgustinMendoza (Colaborador) - Actualmente bajo: JuanGestor, MariaJefa"
   - Si se va a cambiar, mostrar advertencia clara

5. **VALIDACIONES:**
   - No permitir ciclos (A reporta a B, B reporta a A)
   - No permitir que un usuario se asigne a sí mismo
   - Advertir si un usuario ya tiene un superior del mismo tipo

6. **MEJORAS UX:**
   - Pestañas o acordeones colapsables para no saturar la pantalla
   - Búsqueda/filtro en las listas de subordinados
   - Contador de personas asignadas: "Gestores asignados (2/5 disponibles)"
   - Indicador visual de cambios pendientes antes de guardar

7. **LOGS Y AUDITORÍA:**
   - Registrar en consola cada cambio de jerarquía
   - Formato: "[JERARQUÍA] Colaborador AgustinMendoza: GestorId cambió de 5 a 8"

8. **COMPATIBILIDAD:**
   - Al cargar un usuario existente, leer sus campos *Id actuales
   - Mapear a los checkboxes correspondientes
   - Al guardar, actualizar SOLO los *Id que hayan cambiado

ENTREGABLES:
1. Modificar ModalEditarUsuario.razor con la nueva interfaz
2. Actualizar lógica de guardado para mantener sincronización
3. Añadir validaciones de ciclos y consistencia
4. Documentar cambios en comentarios del código
5. Probar con usuarios existentes para verificar compatibilidad

RESULTADO ESPERADO:
- Interfaz clara donde los superiores asignan subordinados
- Subordinados ven su jerarquía pero no la editan directamente
- Eliminación de la confusión de doble asignación (dropdowns vs checkboxes)
- Mantenimiento de todos los datos y cálculos existentes
```

---

## 📋 CHECKLIST DE IMPLEMENTACIÓN

- [ ] Backup de base de datos antes de cambios
- [ ] Modificar ModalEditarUsuario.razor
  - [ ] Eliminar dropdowns de GestorId/JefeVentasId/DirectorComercialId para subordinados
  - [ ] Mantener solo checkboxes en superiores
  - [ ] Añadir sección "Tu Jerarquía" (solo lectura) para subordinados
- [ ] Actualizar lógica de guardado
  - [ ] Al marcar checkbox, actualizar *Id del subordinado
  - [ ] Al desmarcar, poner *Id = null
- [ ] Añadir validaciones
  - [ ] Detectar ciclos
  - [ ] Prevenir auto-asignación
- [ ] Mejorar UX
  - [ ] Pestañas/acordeones
  - [ ] Búsqueda/filtros
  - [ ] Contadores
- [ ] Testing
  - [ ] Probar con usuario sin jerarquía
  - [ ] Probar con usuario con jerarquía existente
  - [ ] Probar cambios de jerarquía
  - [ ] Verificar que comisiones se calculan correctamente
- [ ] Documentación
  - [ ] Comentar código nuevo
  - [ ] Actualizar este documento con resultado final

---

## 🔍 ARCHIVOS A MODIFICAR

1. **Principal:**
   - `/Components/Pages/Usuarios/ModalEditarUsuario.razor` - Interfaz de edición
   - `/Components/Pages/Usuarios/ModalEditarUsuario.razor.cs` (si existe)

2. **Opcional (si se hace página dedicada):**
   - `/Components/Pages/Jerarquia.razor` - Nueva página de árbol visual

3. **Sin cambios (solo referencia):**
   - `/Models/Usuario.cs` - NO cambiar (mantener campos)
   - `/Services/ComisionService.cs` - NO cambiar (mantener lógica)

---

## 💾 USUARIOS EXISTENTES - PLAN DE MIGRACIÓN

**NO SE REQUIERE MIGRACIÓN DE DATOS**

Los campos `GestorId`, `JefeVentasId` y `DirectorComercialId` ya existen y están poblados.
Solo cambia la INTERFAZ de cómo se asignan, no los datos en sí.

**Verificación post-implementación:**
1. Cargar usuario con jerarquía existente
2. Verificar que se muestran correctamente sus superiores (si es subordinado) o subordinados (si es superior)
3. Hacer un cambio y guardar
4. Verificar en base de datos que los campos *Id se actualizaron correctamente
5. Crear un contrato con ese usuario y verificar que las comisiones se distribuyen bien

---

**Fecha de análisis:** 6 de mayo de 2026  
**Estado:** Pendiente de implementación  
**Prioridad:** Alta (mejora de usabilidad crítica)
