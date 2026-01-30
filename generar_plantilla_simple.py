#!/usr/bin/env python3
"""
Script simple para generar la plantilla Excel de importación de clientes
Sin dependencias externas, usa xlsxwriter que suele estar instalado
"""

try:
    import xlsxwriter
except ImportError:
    print("Error: Necesitas instalar xlsxwriter")
    print("Ejecuta: pip3 install xlsxwriter")
    exit(1)

# Crear archivo Excel
archivo = 'plantilla_clientes.xlsx'
workbook = xlsxwriter.Workbook(archivo)

# === HOJA DE CLIENTES ===
worksheet_clientes = workbook.add_worksheet('Clientes')

# Formatos
header_format = workbook.add_format({
    'bold': True,
    'bg_color': '#ADD8E6',
    'align': 'center',
    'valign': 'vcenter',
    'border': 1
})

example_format = workbook.add_format({
    'border': 1
})

# Encabezados
headers = [
    'TipoCliente*', 'Nombre*', 'DNI/CIF', 'DNI Representante', 'Email', 
    'Teléfono', 'Dirección', 'Número', 'Escalera', 'Piso', 
    'Puerta', 'Aclarador', 'Población', 'Provincia', 'Código Postal', 
    'IBAN', 'Representante', 'Comercial', 'Observaciones'
]

# Escribir encabezados
for col, header in enumerate(headers):
    worksheet_clientes.write(0, col, header, header_format)

# Datos de ejemplo
ejemplo = [
    'Particular',                    # TipoCliente*
    'Ejemplo Cliente S.L.',          # Nombre*
    'B12345678',                     # DNI/CIF
    '12345678A',                     # DNI Representante
    'cliente@ejemplo.com',           # Email
    '912345678',                     # Teléfono
    'Calle Mayor',                   # Dirección
    '10',                            # Número
    'A',                             # Escalera
    '3',                             # Piso
    'B',                             # Puerta
    'Junto al parque',               # Aclarador
    'Madrid',                        # Población
    'Madrid',                        # Provincia
    '28001',                         # Código Postal
    'ES1234567890123456789012',      # IBAN
    'Juan Pérez',                    # Representante
    'María García',                  # Comercial
    'Cliente potencial importante'   # Observaciones
]

# Escribir ejemplo
for col, value in enumerate(ejemplo):
    worksheet_clientes.write(1, col, value, example_format)

# Ajustar anchos de columna
column_widths = [15, 25, 12, 18, 25, 12, 20, 8, 10, 8, 8, 20, 15, 15, 15, 28, 20, 20, 30]
for col, width in enumerate(column_widths):
    worksheet_clientes.set_column(col, col, width)

# === HOJA DE INSTRUCCIONES ===
worksheet_instruc = workbook.add_worksheet('Instrucciones')

title_format = workbook.add_format({
    'bold': True,
    'font_size': 14,
    'font_color': '#003366'
})

subtitle_format = workbook.add_format({
    'bold': True,
    'font_color': '#003366'
})

text_format = workbook.add_format({
    'text_wrap': True
})

# Configurar ancho de columna
worksheet_instruc.set_column(0, 0, 100)

# Contenido de instrucciones
row = 0

worksheet_instruc.write(row, 0, 'INSTRUCCIONES PARA IMPORTAR CLIENTES', title_format)
row += 2

worksheet_instruc.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
row += 1
worksheet_instruc.write(row, 0, '- TipoCliente: Debe ser exactamente "Particular" o "Empresa"')
row += 1
worksheet_instruc.write(row, 0, '- Nombre: Nombre o razón social del cliente')
row += 2

worksheet_instruc.write(row, 0, 'Campos opcionales:', subtitle_format)
row += 1
instrucciones = [
    '- DNI/CIF: Documento de identificación',
    '- DNI Representante: DNI del representante legal',
    '- Email: Correo electrónico (se valida el formato)',
    '- Teléfono: Número de teléfono de contacto',
    '- Dirección, Número, Escalera, Piso, Puerta: Dirección completa del cliente',
    '- Aclarador: Información adicional de ubicación (ej: "Junto al parque")',
    '- Población, Provincia, Código Postal: Datos de localización',
    '- IBAN: Cuenta bancaria (máximo 34 caracteres)',
    '- Representante: Nombre del representante legal',
    '- Comercial: Nombre del comercial asignado al cliente',
    '- Observaciones: Notas adicionales sobre el cliente'
]
for inst in instrucciones:
    worksheet_instruc.write(row, 0, inst)
    row += 1

row += 1
worksheet_instruc.write(row, 0, 'Notas importantes:', subtitle_format)
row += 1
notas = [
    '- NO elimine la primera fila de encabezados en la hoja "Clientes"',
    '- Puede eliminar la fila de ejemplo antes de importar sus datos',
    '- Añada tantas filas como clientes necesite importar',
    '- Los campos vacíos se guardarán como NULL en la base de datos',
    '- La fecha de alta se asignará automáticamente al momento de importar',
    '- El formato del archivo debe ser .xlsx'
]
for nota in notas:
    worksheet_instruc.write(row, 0, nota)
    row += 1

row += 1
worksheet_instruc.write(row, 0, 'Validaciones que realiza el script:', subtitle_format)
row += 1
validaciones = [
    '- TipoCliente debe ser "Particular" o "Empresa" (exactamente, respetando mayúsculas)',
    '- Nombre es obligatorio y no puede estar vacío',
    '- Email debe tener un formato válido si se proporciona',
    '- IBAN no puede exceder 34 caracteres',
    '- Todos los campos de texto respetan los límites de la base de datos'
]
for val in validaciones:
    worksheet_instruc.write(row, 0, val)
    row += 1

row += 1
worksheet_instruc.write(row, 0, 'Ejemplos de valores válidos:', subtitle_format)
row += 1
ejemplos = [
    'TipoCliente: "Particular" o "Empresa"',
    'Nombre: "Juan Pérez García" o "Empresa Ejemplo S.L."',
    'Email: "ejemplo@dominio.com"',
    'IBAN: "ES1234567890123456789012"'
]
for ej in ejemplos:
    worksheet_instruc.write(row, 0, ej)
    row += 1

row += 2
worksheet_instruc.write(row, 0, 'Para más información, consulte el archivo IMPORTACION_CLIENTES.md')

# Cerrar archivo
workbook.close()

print(f"✓ Plantilla creada exitosamente: {archivo}")
print(f"✓ Incluye hoja 'Clientes' con ejemplo y hoja 'Instrucciones' con ayuda detallada")
print(f"\nPróximos pasos:")
print(f"1. Abre {archivo} y rellena tus datos en la hoja 'Clientes'")
print(f"2. Configura la conexión a la base de datos en importar_clientes.py")
print(f"3. Ejecuta: python3 importar_clientes.py {archivo}")
