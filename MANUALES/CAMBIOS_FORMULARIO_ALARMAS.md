# Actualización del Formulario de Alarmas

## Fecha: 29/12/2025

## Cambios realizados:

### 1. **Ampliación de Subtipos de Negocio en Alarmas**

Se han añadido todas las opciones de subtipos para negocios según los requisitos:

**Opciones añadidas:**
- Comercio
- Bar
- Restaurante
- Taller
- **Estanco** (nuevo)
- Farmacia
- Clínica
- Despacho
- **Inmobiliaria** (nuevo)
- **Constructora** (nuevo)
- **Banco** (nuevo)
- **Gasolinera** (nuevo)
- **Galería de Arte** (nuevo)
- **Loterías** (nuevo)
- **Salón de Juegos** (nuevo)
- **Joyería** (nuevo)
- **Seguridad** (nuevo)
- **Armero** (nuevo)
- **Centro Universitario** (nuevo)
- **Interiorismo y Decoración** (nuevo)

**Archivo modificado:** `Components/Pages/ContratosAlarmas.razor`

### 2. **Campo Fecha de Permanencia**

Se ha añadido el campo "Fecha de Permanencia" en la sección de contrato anterior.

**Ubicación:** Sección "Contrato Anterior" del formulario de alarmas
**Tipo:** InputDate (selector de fecha)
**Condición:** Solo visible cuando "¿Tiene contrato anterior?" está marcado

**Archivo modificado:** `Components/Pages/ContratosAlarmas.razor`

### 3. **Campo Copia Recibo Bancario en Clientes**

Se ha añadido el campo "Copia Recibo Bancario" en el formulario de clientes.

**Ubicación:** Después del campo IBAN en el formulario de edición de clientes
**Nota:** Indicado como obligatorio para contratos de Alarmas
**Tipo:** Texto (puede contener ruta o referencia del archivo)

**Archivo modificado:** `Components/Pages/Clientes.razor`

## Estado de los campos:

### ✅ Campos ya implementados previamente:

1. **Tipo de Alarma** (HOGAR/NEGOCIO) - ✅ Implementado
2. **Subtipo de Inmueble** (Desplegable según tipo) - ✅ Implementado y ampliado
3. **¿Tiene contrato anterior?** - ✅ Implementado
4. **Compañía anterior** - ✅ Implementado
5. **Número de contrato anterior** - ✅ Implementado
6. **Fecha de permanencia** - ✅ Añadido
7. **Kit** (desplegable de tarifas) - ✅ Implementado
8. **Opcionales** - ✅ Implementado
9. **Campaña** (desplegable de tarifas) - ✅ Implementado
10. **Dirección de instalación** - ✅ Implementado
11. **Observaciones** - ✅ Implementado
12. **Copia recibo bancario** - ✅ Añadido en formulario de clientes

## Modelo de datos:

Todos los campos ya estaban en el modelo `Contrato.cs`:
- `TipoAlarma` (string)
- `SubtipoInmueble` (string)
- `TieneContratoAnterior` (bool?)
- `CompaniaAnterior` (string)
- `NumeroContratoAnterior` (string)
- `FechaPermanenciaAnterior` (DateTime?)
- `KitAlarma` (string)
- `OpcionalesAlarma` (string)
- `CampanaAlarma` (string)
- `DireccionInstalacionAlarma` (string)
- `ObservacionesAlarma` (string)

Campo en modelo `Cliente.cs`:
- `CopiaReciboBancario` (string)

## Notas importantes:

1. Los desplegables de Kit y Campaña se alimentan de la tabla `tarifasalarmas`
2. El campo "Copia Recibo Bancario" es un campo de texto que puede almacenar la ruta o referencia del archivo
3. No se requieren cambios en la base de datos, todos los campos ya existen
4. La aplicación está ejecutándose correctamente en http://localhost:5169
