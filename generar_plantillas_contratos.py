#!/usr/bin/env python3
"""
Script para generar las plantillas Excel de importación de contratos
Genera 3 plantillas: Energía, Telefonía y Alarmas
"""

try:
    import xlsxwriter
except ImportError:
    print("Error: Necesitas instalar xlsxwriter")
    print("Ejecuta: pip3 install --user xlsxwriter")
    exit(1)

def crear_plantilla_energia():
    """Crea la plantilla para contratos de energía"""
    archivo = 'plantilla_contratos_energia.xlsx'
    workbook = xlsxwriter.Workbook(archivo)
    
    # Formatos
    header_format = workbook.add_format({
        'bold': True,
        'bg_color': '#90EE90',
        'align': 'center',
        'valign': 'vcenter',
        'border': 1
    })
    
    example_format = workbook.add_format({'border': 1})
    
    # === HOJA DE CONTRATOS ===
    worksheet = workbook.add_worksheet('Contratos Energía')
    
    # Encabezados
    headers = [
        'IdCliente*', 'Estado', 'Comercial', 'FechaAlta', 
        'EstadoServicio', 'Comercializadora', 'Tarifa', 'CUPS', 
        'CUPSGas', 'Servicios', 'IBAN', 'TipoOperacion',
        'PotenciaContratada', 'ConsumoAnual', 'Comision', 'Observaciones'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Datos de ejemplo
    ejemplo = [
        '1',                    # IdCliente*
        'Pendiente',            # Estado
        'Juan García',          # Comercial
        '2026-01-29',          # FechaAlta
        'Activo',              # EstadoServicio
        'Iberdrola',           # Comercializadora
        'Tarifa 2.0TD',        # Tarifa
        'ES0031234567890123456789012', # CUPS
        'ES0041234567890123456789012', # CUPSGas
        'Luz + Gas',           # Servicios
        'ES1234567890123456789012',    # IBAN
        'Alta',                # TipoOperacion
        '4.6',                 # PotenciaContratada
        '3500',                # ConsumoAnual
        '50.00',               # Comision
        'Cliente preferente'   # Observaciones
    ]
    
    for col, value in enumerate(ejemplo):
        worksheet.write(1, col, value, example_format)
    
    # Ajustar anchos
    column_widths = [12, 15, 20, 12, 15, 20, 20, 30, 30, 15, 28, 15, 18, 15, 12, 30]
    for col, width in enumerate(column_widths):
        worksheet.set_column(col, col, width)
    
    # === HOJA DE INSTRUCCIONES ===
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#006400'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#006400'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN CONTRATOS DE ENERGÍA', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
    row += 1
    worksheet_inst.write(row, 0, '- IdCliente*: ID del cliente en la tabla clientes_simple (debe existir previamente)')
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos opcionales:', subtitle_format)
    row += 1
    instrucciones = [
        '- Estado: Estado del contrato (Pendiente, Activo, Cancelado, etc.)',
        '- Comercial: Nombre del comercial',
        '- FechaAlta: Fecha de alta en formato YYYY-MM-DD',
        '- EstadoServicio: Estado del servicio',
        '- Comercializadora: Nombre de la comercializadora',
        '- Tarifa: Tarifa contratada',
        '- CUPS: Código CUPS de luz (máx. 30 caracteres)',
        '- CUPSGas: Código CUPS de gas (máx. 30 caracteres)',
        '- Servicios: Servicios contratados',
        '- IBAN: Cuenta bancaria',
        '- TipoOperacion: Alta, Cambio de titular, etc.',
        '- PotenciaContratada: Potencia en kW (decimal)',
        '- ConsumoAnual: Consumo anual en kWh (decimal)',
        '- Comision: Comisión del contrato (decimal)',
        '- Observaciones: Notas adicionales'
    ]
    for inst in instrucciones:
        worksheet_inst.write(row, 0, inst)
        row += 1
    
    workbook.close()
    print(f"✓ Plantilla creada: {archivo}")
    return archivo

def crear_plantilla_telefonia():
    """Crea la plantilla para contratos de telefonía"""
    archivo = 'plantilla_contratos_telefonia.xlsx'
    workbook = xlsxwriter.Workbook(archivo)
    
    # Formatos
    header_format = workbook.add_format({
        'bold': True,
        'bg_color': '#87CEEB',
        'align': 'center',
        'valign': 'vcenter',
        'border': 1
    })
    
    example_format = workbook.add_format({'border': 1})
    
    # === HOJA DE CONTRATOS ===
    worksheet = workbook.add_worksheet('Contratos Telefonía')
    
    # Encabezados
    headers = [
        'IdCliente*', 'Estado', 'Comercial', 'FechaAlta', 
        'Operadora', 'Tarifa', 'TipoTarifa', 'FijoTel',
        'LineaMovilPrincipal', 'TipoLineaMovilPrincipal', 'CodigoICCPrincipal',
        'NumeroLineas', 'IBAN', 'Comision', 'Observaciones'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Datos de ejemplo
    ejemplo = [
        '1',                    # IdCliente*
        'Pendiente',           # Estado
        'María López',         # Comercial
        '2026-01-29',         # FechaAlta
        'Movistar',           # Operadora
        'Tarifa Fusión',      # Tarifa
        'Fibra 600Mb',        # TipoTarifa
        '912345678',          # FijoTel
        '654321987',          # LineaMovilPrincipal
        'Contrato',           # TipoLineaMovilPrincipal
        '8934071234567890123', # CodigoICCPrincipal
        '2',                  # NumeroLineas
        'ES1234567890123456789012', # IBAN
        '40.00',              # Comision
        'Cliente VIP'         # Observaciones
    ]
    
    for col, value in enumerate(ejemplo):
        worksheet.write(1, col, value, example_format)
    
    # Ajustar anchos
    column_widths = [12, 15, 20, 12, 15, 20, 18, 15, 18, 22, 22, 12, 28, 12, 30]
    for col, width in enumerate(column_widths):
        worksheet.set_column(col, col, width)
    
    # === HOJA DE INSTRUCCIONES ===
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#00008B'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#00008B'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN CONTRATOS DE TELEFONÍA', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
    row += 1
    worksheet_inst.write(row, 0, '- IdCliente*: ID del cliente en la tabla clientes_simple (debe existir previamente)')
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos opcionales:', subtitle_format)
    row += 1
    instrucciones = [
        '- Estado: Estado del contrato',
        '- Comercial: Nombre del comercial',
        '- FechaAlta: Fecha de alta en formato YYYY-MM-DD',
        '- Operadora: Nombre de la operadora',
        '- Tarifa: Tarifa contratada',
        '- TipoTarifa: Tipo de tarifa (Fibra, Móvil, etc.)',
        '- FijoTel: Número de teléfono fijo',
        '- LineaMovilPrincipal: Número de móvil principal',
        '- TipoLineaMovilPrincipal: "Contrato" o "Prepago"',
        '- CodigoICCPrincipal: Código ICC/SIM (19 dígitos)',
        '- NumeroLineas: Número de líneas móviles adicionales',
        '- IBAN: Cuenta bancaria',
        '- Comision: Comisión del contrato',
        '- Observaciones: Notas adicionales'
    ]
    for inst in instrucciones:
        worksheet_inst.write(row, 0, inst)
        row += 1
    
    row += 1
    worksheet_inst.write(row, 0, 'Nota: Para líneas adicionales, use la hoja "Líneas Adicionales"', subtitle_format)
    
    # === HOJA DE LÍNEAS ADICIONALES ===
    worksheet_lineas = workbook.add_worksheet('Líneas Adicionales')
    
    headers_lineas = ['IdCliente*', 'NumeroLinea', 'Telefono', 'Tarifa', 'TipoLinea', 'CodigoICC']
    
    for col, header in enumerate(headers_lineas):
        worksheet_lineas.write(0, col, header, header_format)
    
    # Ejemplo
    ejemplo_linea = ['1', '1', '622334455', 'Tarifa 20GB', 'Contrato', '8934071234567890456']
    for col, value in enumerate(ejemplo_linea):
        worksheet_lineas.write(1, col, value, example_format)
    
    column_widths_lineas = [12, 15, 15, 25, 15, 22]
    for col, width in enumerate(column_widths_lineas):
        worksheet_lineas.set_column(col, col, width)
    
    workbook.close()
    print(f"✓ Plantilla creada: {archivo}")
    return archivo

def crear_plantilla_alarmas():
    """Crea la plantilla para contratos de alarmas"""
    archivo = 'plantilla_contratos_alarmas.xlsx'
    workbook = xlsxwriter.Workbook(archivo)
    
    # Formatos
    header_format = workbook.add_format({
        'bold': True,
        'bg_color': '#FFB6C1',
        'align': 'center',
        'valign': 'vcenter',
        'border': 1
    })
    
    example_format = workbook.add_format({'border': 1})
    
    # === HOJA DE CONTRATOS ===
    worksheet = workbook.add_worksheet('Contratos Alarmas')
    
    # Encabezados
    headers = [
        'IdCliente*', 'Estado', 'Comercial', 'FechaAlta',
        'TipoAlarma', 'SubtipoInmueble', 'EmpresaAlarma', 'KitAlarma',
        'OpcionalesAlarma', 'CampanaAlarma', 'TieneContratoAnterior',
        'CompaniaAnterior', 'DireccionInstalacion', 'NumeroInstalacion',
        'PisoInstalacion', 'CodigoPostalInstalacion', 'ProvinciaInstalacion',
        'LocalidadInstalacion', 'IBAN', 'Comision', 'Observaciones'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Datos de ejemplo
    ejemplo = [
        '1',                    # IdCliente*
        'Pendiente',           # Estado
        'Pedro Sánchez',       # Comercial
        '2026-01-29',         # FechaAlta
        'Hogar',              # TipoAlarma (Hogar o Negocio)
        'Piso',               # SubtipoInmueble
        'Securitas Direct',   # EmpresaAlarma
        'Kit Básico',         # KitAlarma
        'Detector humo',      # OpcionalesAlarma
        'Campaña Enero 2026', # CampanaAlarma
        'No',                 # TieneContratoAnterior (Sí/No)
        '',                   # CompaniaAnterior
        'Calle Ejemplo',      # DireccionInstalacion
        '25',                 # NumeroInstalacion
        '3B',                 # PisoInstalacion
        '28001',              # CodigoPostalInstalacion
        'Madrid',             # ProvinciaInstalacion
        'Madrid',             # LocalidadInstalacion
        'ES1234567890123456789012', # IBAN
        '60.00',              # Comision
        'Instalación urgente' # Observaciones
    ]
    
    for col, value in enumerate(ejemplo):
        worksheet.write(1, col, value, example_format)
    
    # Ajustar anchos
    column_widths = [12, 15, 20, 12, 12, 18, 20, 20, 20, 20, 22, 20, 25, 18, 15, 22, 20, 20, 28, 12, 30]
    for col, width in enumerate(column_widths):
        worksheet.set_column(col, col, width)
    
    # === HOJA DE INSTRUCCIONES ===
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#8B0000'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#8B0000'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN CONTRATOS DE ALARMAS', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
    row += 1
    worksheet_inst.write(row, 0, '- IdCliente*: ID del cliente en la tabla clientes_simple (debe existir previamente)')
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos opcionales:', subtitle_format)
    row += 1
    instrucciones = [
        '- Estado: Estado del contrato',
        '- Comercial: Nombre del comercial',
        '- FechaAlta: Fecha de alta en formato YYYY-MM-DD',
        '- TipoAlarma: "Hogar" o "Negocio"',
        '- SubtipoInmueble: Piso, Bajo, Chalet, Local, etc.',
        '- EmpresaAlarma: Nombre de la empresa de alarmas',
        '- KitAlarma: Kit contratado',
        '- OpcionalesAlarma: Opcionales adicionales',
        '- CampanaAlarma: Campaña comercial',
        '- TieneContratoAnterior: "Sí" o "No"',
        '- CompaniaAnterior: Compañía anterior si aplica',
        '- DireccionInstalacion: Dirección de instalación',
        '- NumeroInstalacion: Número del inmueble',
        '- PisoInstalacion: Piso/Puerta',
        '- CodigoPostalInstalacion: Código postal',
        '- ProvinciaInstalacion: Provincia',
        '- LocalidadInstalacion: Localidad',
        '- IBAN: Cuenta bancaria',
        '- Comision: Comisión del contrato',
        '- Observaciones: Notas adicionales'
    ]
    for inst in instrucciones:
        worksheet_inst.write(row, 0, inst)
        row += 1
    
    workbook.close()
    print(f"✓ Plantilla creada: {archivo}")
    return archivo

if __name__ == "__main__":
    print("="*60)
    print("GENERADOR DE PLANTILLAS DE CONTRATOS")
    print("="*60)
    print()
    
    archivos = []
    
    print("Generando plantilla de Energía...")
    archivos.append(crear_plantilla_energia())
    
    print("Generando plantilla de Telefonía...")
    archivos.append(crear_plantilla_telefonia())
    
    print("Generando plantilla de Alarmas...")
    archivos.append(crear_plantilla_alarmas())
    
    print()
    print("="*60)
    print("✅ PLANTILLAS CREADAS EXITOSAMENTE")
    print("="*60)
    print()
    print("Archivos generados:")
    for archivo in archivos:
        print(f"  - {archivo}")
    print()
    print("Próximos pasos:")
    print("1. Abre las plantillas y rellena los datos de tus contratos")
    print("2. Configura la conexión a la base de datos en importar_contratos.py")
    print("3. Ejecuta: python3 importar_contratos.py <tipo> <archivo>")
    print("   Ejemplo: python3 importar_contratos.py energia plantilla_contratos_energia.xlsx")
