#!/usr/bin/env python3
"""
Script para generar las plantillas Excel de importación de tarifas y servicios
Genera 4 plantillas: Tarifas Energía, Tarifas Telefonía, Tarifas Alarmas y Servicios
"""

try:
    import xlsxwriter
except ImportError:
    print("Error: Necesitas instalar xlsxwriter")
    print("Ejecuta: pip3 install --user xlsxwriter")
    exit(1)

def crear_plantilla_tarifa_energia():
    """Crea la plantilla para tarifas de energía"""
    archivo = 'plantilla_tarifas_energia.xlsx'
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
    
    # === HOJA DE TARIFAS ===
    worksheet = workbook.add_worksheet('Tarifas Energía')
    
    # Encabezados
    headers = [
        'Empresa*', 'Tipo*', 'Nombre*', 'Potencia1*', 'Energia1*',
        'Precio', 'Potencia2', 'Potencia3', 'Potencia4', 'Potencia5', 'Potencia6',
        'Energia2', 'Energia3', 'Energia4', 'Energia5', 'Energia6',
        'Comision*', 'PrecioNew*', 'TerminoFijoDiario', 'PrecioPotenciaP1',
        'PrecioPotenciaP2', 'PrecioPotenciaP3', 'PrecioEnergiaP1', 
        'PrecioEnergiaP2', 'PrecioEnergiaP3'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Datos de ejemplo
    ejemplo = [
        'Iberdrola', 'Luz', 'Tarifa 2.0TD Estable', '0.12', '0.15',
        '45.50', '0.10', '', '', '', '',
        '0.13', '', '', '', '',
        '50.00', '45.50', '0.15', '0.12',
        '0.10', '', '0.15', '0.13', ''
    ]
    
    for col, value in enumerate(ejemplo):
        worksheet.write(1, col, value, example_format)
    
    # Ajustar anchos
    column_widths = [15, 12, 30, 12, 12, 10, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 18, 18, 18, 18, 18, 18, 18]
    for col, width in enumerate(column_widths):
        worksheet.set_column(col, col, width)
    
    # === HOJA DE INSTRUCCIONES ===
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#006400'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#006400'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN TARIFAS DE ENERGÍA', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
    row += 1
    instrucciones = [
        '- Empresa*: Nombre de la comercializadora (Iberdrola, Endesa, Naturgy, etc.)',
        '- Tipo*: Tipo de tarifa (Luz, Gas, Luz+Gas)',
        '- Nombre*: Nombre de la tarifa',
        '- Potencia1*: Precio potencia periodo 1 (texto)',
        '- Energia1*: Precio energía periodo 1 (texto)',
        '- Comision*: Comisión de la tarifa (decimal)',
        '- PrecioNew*: Precio mensual estimado (decimal)',
        '',
        'Campos opcionales:',
        '- Precio: Precio alternativo (texto)',
        '- Potencia2-6: Precios potencia periodos adicionales (texto)',
        '- Energia2-6: Precios energía periodos adicionales (texto)',
        '- TerminoFijoDiario: Término fijo diario (decimal)',
        '- PrecioPotenciaP1-P3: Precio potencia por periodo (decimal)',
        '- PrecioEnergiaP1-P3: Precio energía por periodo (decimal)',
        '',
        'Notas:',
        '- Los campos Potencia y Energia son texto, pueden contener descripciones',
        '- Los campos con "New" y precios específicos son decimales (usar punto como separador)',
        '- Campos de comparativa (TerminoFijo, PrecioPotencia, PrecioEnergia) son opcionales'
    ]
    for inst in instrucciones:
        worksheet_inst.write(row, 0, inst)
        row += 1
    
    workbook.close()
    print(f"✓ Plantilla creada: {archivo}")
    return archivo

def crear_plantilla_tarifa_telefonia():
    """Crea la plantilla para tarifas de telefonía"""
    archivo = 'plantilla_tarifas_telefonia.xlsx'
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
    
    # === HOJA DE TARIFAS ===
    worksheet = workbook.add_worksheet('Tarifas Telefonía')
    
    # Encabezados
    headers = [
        'Compania*', 'Tipo*', 'Fibra', 'GbMovil',
        'Precio', 'Comision', 'PrecioNew*', 'ComisionNew*', 'TV'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Datos de ejemplo
    ejemplo = [
        'Movistar', 'Fibra + Móvil', '600Mb', '50GB',
        '49.90', '40.00', '49.90', '40.00', 'Netflix incluido'
    ]
    
    for col, value in enumerate(ejemplo):
        worksheet.write(1, col, value, example_format)
    
    # Ajustar anchos
    column_widths = [20, 20, 15, 15, 12, 12, 15, 15, 25]
    for col, width in enumerate(column_widths):
        worksheet.set_column(col, col, width)
    
    # === HOJA DE INSTRUCCIONES ===
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#00008B'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#00008B'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN TARIFAS DE TELEFONÍA', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
    row += 1
    instrucciones = [
        '- Compania*: Nombre de la operadora (Movistar, Vodafone, Orange, MásMóvil, etc.)',
        '- Tipo*: Tipo de tarifa (Solo Fibra, Solo Móvil, Fibra + Móvil, Convergente)',
        '- PrecioNew*: Precio mensual (decimal, usar punto como separador)',
        '- ComisionNew*: Comisión de la tarifa (decimal)',
        '',
        'Campos opcionales:',
        '- Fibra: Velocidad de fibra (100Mb, 300Mb, 600Mb, 1Gb)',
        '- GbMovil: Datos móviles incluidos (20GB, 50GB, Ilimitado)',
        '- Precio: Precio alternativo (texto)',
        '- Comision: Comisión alternativa (texto)',
        '- TV: Servicios de TV incluidos',
        '',
        'Ejemplos:',
        'Compania: Movistar, Vodafone, Orange',
        'Tipo: Fibra + Móvil, Solo Fibra, Solo Móvil',
        'Fibra: 600Mb, 1Gb',
        'GbMovil: 50GB, 100GB, Ilimitado'
    ]
    for inst in instrucciones:
        worksheet_inst.write(row, 0, inst)
        row += 1
    
    workbook.close()
    print(f"✓ Plantilla creada: {archivo}")
    return archivo

def crear_plantilla_tarifa_alarmas():
    """Crea la plantilla para tarifas de alarmas"""
    archivo = 'plantilla_tarifas_alarmas.xlsx'
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
    
    # === HOJA DE TARIFAS ===
    worksheet = workbook.add_worksheet('Tarifas Alarmas')
    
    # Encabezados
    headers = [
        'Tipo*', 'TipoInmueble*', 'NombreTarifa*', 'CuotaMensual*',
        'Permanencia*', 'Empresa', 'Comision', 'Descripcion', 'Activa'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Datos de ejemplo
    ejemplo = [
        'Kit', 'Hogar', 'Kit Básico Hogar', '29.90',
        '24', 'Securitas Direct', '50.00', 'Kit básico con 2 sensores', 'Sí'
    ]
    
    for col, value in enumerate(ejemplo):
        worksheet.write(1, col, value, example_format)
    
    # Ejemplo 2
    ejemplo2 = [
        'Opcional', 'Hogar', 'Detector de Humo', '3.50',
        '0', 'Securitas Direct', '0', 'Detector adicional', 'Sí'
    ]
    for col, value in enumerate(ejemplo2):
        worksheet.write(2, col, value, example_format)
    
    # Ejemplo 3
    ejemplo3 = [
        'Campaña', 'Negocio', 'Promoción Verano', '39.90',
        '36', 'Prosegur', '80.00', 'Oferta especial negocios', 'Sí'
    ]
    for col, value in enumerate(ejemplo3):
        worksheet.write(3, col, value, example_format)
    
    # Ajustar anchos
    column_widths = [15, 18, 30, 15, 15, 20, 12, 40, 10]
    for col, width in enumerate(column_widths):
        worksheet.set_column(col, col, width)
    
    # === HOJA DE INSTRUCCIONES ===
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#8B0000'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#8B0000'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN TARIFAS DE ALARMAS', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
    row += 1
    instrucciones = [
        '- Tipo*: Tipo de tarifa - "Kit", "Opcional" o "Campaña"',
        '- TipoInmueble*: Tipo de inmueble - "Hogar" o "Negocio"',
        '- NombreTarifa*: Nombre descriptivo de la tarifa',
        '- CuotaMensual*: Cuota mensual (decimal, usar punto como separador)',
        '- Permanencia*: Meses de permanencia (número entero: 0, 12, 24, 36)',
        '',
        'Campos opcionales:',
        '- Empresa: Nombre de la empresa de alarmas (Securitas Direct, Prosegur, etc.)',
        '- Comision: Comisión de la tarifa (decimal)',
        '- Descripcion: Descripción detallada de la tarifa',
        '- Activa: Estado de la tarifa - "Sí" o "No" (por defecto Sí)',
        '',
        'Tipos de tarifas:',
        '- Kit: Kits completos de alarma (ej: Kit Básico, Kit Premium)',
        '- Opcional: Elementos opcionales (ej: Detector humo, Cámara adicional)',
        '- Campaña: Ofertas y campañas especiales',
        '',
        'Ejemplos:',
        'Kit Hogar: Kit Básico Hogar, 29.90€, 24 meses',
        'Opcional: Detector de Humo, 3.50€, 0 meses',
        'Campaña: Promoción Verano, 39.90€, 36 meses'
    ]
    for inst in instrucciones:
        worksheet_inst.write(row, 0, inst)
        row += 1
    
    workbook.close()
    print(f"✓ Plantilla creada: {archivo}")
    return archivo

def crear_plantilla_servicios():
    """Crea la plantilla para servicios"""
    archivo = 'plantilla_servicios.xlsx'
    workbook = xlsxwriter.Workbook(archivo)
    
    # Formatos
    header_format = workbook.add_format({
        'bold': True,
        'bg_color': '#FFD700',
        'align': 'center',
        'valign': 'vcenter',
        'border': 1
    })
    
    example_format = workbook.add_format({'border': 1})
    
    # === HOJA DE SERVICIOS ===
    worksheet = workbook.add_worksheet('Servicios')
    
    # Encabezados
    headers = ['Tipo*', 'NombreServicio*', 'Precio*', 'Empresa']
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Datos de ejemplo
    ejemplos = [
        ['Energía', 'Mantenimiento caldera anual', '120.00', 'Iberdrola'],
        ['Telefonía', 'Router WiFi 6', '5.00', 'Movistar'],
        ['Alarmas', 'Mantenimiento preventivo', '50.00', 'Securitas'],
        ['General', 'Consultoría energética', '200.00', '']
    ]
    
    for row_idx, ejemplo in enumerate(ejemplos, start=1):
        for col, value in enumerate(ejemplo):
            worksheet.write(row_idx, col, value, example_format)
    
    # Ajustar anchos
    column_widths = [15, 40, 12, 20]
    for col, width in enumerate(column_widths):
        worksheet.set_column(col, col, width)
    
    # === HOJA DE INSTRUCCIONES ===
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#B8860B'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#B8860B'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN DE SERVICIOS', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios (marcados con *):', subtitle_format)
    row += 1
    instrucciones = [
        '- Tipo*: Tipo de servicio (Energía, Telefonía, Alarmas, General)',
        '- NombreServicio*: Nombre descriptivo del servicio',
        '- Precio*: Precio del servicio (texto, puede incluir "/mes", "/año", etc.)',
        '',
        'Campos opcionales:',
        '- Empresa: Empresa que ofrece el servicio',
        '',
        'Ejemplos de servicios:',
        '',
        'Energía:',
        '- Mantenimiento caldera anual',
        '- Certificado energético',
        '- Auditoría energética',
        '',
        'Telefonía:',
        '- Router WiFi 6',
        '- Repetidor señal',
        '- Línea adicional',
        '',
        'Alarmas:',
        '- Mantenimiento preventivo',
        '- Cámara adicional',
        '- Sensor de movimiento',
        '',
        'General:',
        '- Consultoría',
        '- Gestoría administrativa',
        '- Asesoramiento legal',
        '',
        'Notas:',
        '- El precio es texto, puede incluir información adicional',
        '- Ejemplos de precio: "120.00", "5.00/mes", "Gratis primer año"'
    ]
    for inst in instrucciones:
        worksheet_inst.write(row, 0, inst)
        row += 1
    
    workbook.close()
    print(f"✓ Plantilla creada: {archivo}")
    return archivo

if __name__ == "__main__":
    print("="*60)
    print("GENERADOR DE PLANTILLAS DE TARIFAS Y SERVICIOS")
    print("="*60)
    print()
    
    archivos = []
    
    print("Generando plantilla de Tarifas Energía...")
    archivos.append(crear_plantilla_tarifa_energia())
    
    print("Generando plantilla de Tarifas Telefonía...")
    archivos.append(crear_plantilla_tarifa_telefonia())
    
    print("Generando plantilla de Tarifas Alarmas...")
    archivos.append(crear_plantilla_tarifa_alarmas())
    
    print("Generando plantilla de Servicios...")
    archivos.append(crear_plantilla_servicios())
    
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
    print("1. Abre las plantillas y rellena los datos")
    print("2. Configura la conexión a la base de datos en importar_tarifas_servicios.py")
    print("3. Ejecuta: python3 importar_tarifas_servicios.py <tipo> <archivo>")
    print("   Ejemplos:")
    print("   python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx")
    print("   python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx")
    print("   python3 importar_tarifas_servicios.py tarifa-alarmas plantilla_tarifas_alarmas.xlsx")
    print("   python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx")
