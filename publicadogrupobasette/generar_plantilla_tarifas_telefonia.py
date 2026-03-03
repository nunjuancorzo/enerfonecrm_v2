#!/usr/bin/env python3
"""
Script para generar una plantilla Excel para importar tarifas de telefonía
Uso: python3 generar_plantilla_tarifas_telefonia.py
"""

import xlsxwriter
from datetime import datetime

def generar_plantilla():
    """Genera un archivo Excel con la plantilla para importar tarifas de telefonía"""
    
    # Crear archivo Excel
    workbook = xlsxwriter.Workbook('plantilla_tarifas_telefonia_importacion.xlsx')
    worksheet = workbook.add_worksheet('Tarifas Telefonía')
    
    # Formatos
    header_format = workbook.add_format({
        'bold': True,
        'bg_color': '#4472C4',
        'font_color': 'white',
        'border': 1
    })
    
    example_format = workbook.add_format({
        'bg_color': '#E7E6E6',
        'border': 1
    })
    
    # Encabezados
    headers = [
        'ID',
        'OPERADORA',
        'TIPO',
        'TARIFA',
        'FIBRA',
        'MOVIL 1',
        'MOVIL 2',
        'TV1',
        'TV2',
        'PRECIO',
        'COMISION',
        'PERMANENCIA',
        'FECHA CARGA'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
        worksheet.set_column(col, col, 18)
    
    # Ejemplos de datos
    ejemplos = [
        ['', 'Movistar', 'Fibra+Móvil', 'Fibra 600Mb + 80GB', '600 Mb', '80 GB', '', 'Netflix', '', '45,00', '50,00', '12 meses', '2026-02-24'],
        ['', 'Orange', 'Solo Fibra', 'Fibra 1Gb', '1 Gb', '', '', '', '', '35,00', '40,00', 'Sin permanencia', '2026-02-24'],
        ['', 'Vodafone', 'Móvil', 'Tarifa Móvil 50GB', '', '50 GB', '', '', '', '20,00', '25,00', '6 meses', '2026-02-24']
    ]
    
    for row_idx, ejemplo in enumerate(ejemplos, start=1):
        for col_idx, value in enumerate(ejemplo):
            worksheet.write(row_idx, col_idx, value, example_format)
    
    # Hoja de instrucciones
    instrucciones_sheet = workbook.add_worksheet('Instrucciones')
    
    instrucciones = [
        ['INSTRUCCIONES PARA IMPORTAR TARIFAS DE TELEFONÍA'],
        [''],
        ['Campos obligatorios:'],
        ['- OPERADORA: Nombre de la operadora (Movistar, Orange, Vodafone, etc.)'],
        ['- TIPO: Tipo de tarifa (Fibra+Móvil, Solo Fibra, Móvil, etc.)'],
        [''],
        ['Campo especial:'],
        ['- ID: Dejar vacío para tarifas nuevas. Incluir el ID para actualizar tarifas existentes.'],
        [''],
        ['Campos opcionales:'],
        ['- TARIFA: Nombre de la tarifa'],
        ['- FIBRA: Velocidad de fibra (600 Mb, 1 Gb, etc.)'],
        ['- MOVIL 1: GB del primer móvil'],
        ['- MOVIL 2: GB del segundo móvil'],
        ['- TV1: Primer servicio de TV'],
        ['- TV2: Segundo servicio de TV'],
        ['- PRECIO: Precio mensual (sin símbolo €)'],
        ['- COMISION: Comisión (sin símbolo €)'],
        ['- PERMANENCIA: Permanencia del contrato'],
        ['- FECHA CARGA: Fecha de carga (formato: YYYY-MM-DD)'],
        [''],
        ['Notas:'],
        ['- La primera fila con los encabezados NO se debe eliminar'],
        ['- Los campos OPERADORA y TIPO son obligatorios'],
        ['- Los precios pueden llevar comas o puntos como separadores decimales'],
        ['- Las filas completamente vacías se ignorarán'],
    ]
    
    title_format = workbook.add_format({
        'bold': True,
        'font_size': 14,
        'font_color': '#4472C4'
    })
    
    section_format = workbook.add_format({
        'bold': True,
        'font_color': '#333333'
    })
    
    # Escribir instrucciones
    for row_idx, line in enumerate(instrucciones):
        content = line[0] if line else ''
        if row_idx == 0:
            instrucciones_sheet.write(row_idx, 0, content, title_format)
        elif content.endswith(':'):
            instrucciones_sheet.write(row_idx, 0, content, section_format)
        else:
            instrucciones_sheet.write(row_idx, 0, content)
    
    instrucciones_sheet.set_column(0, 0, 80)
    
    workbook.close()
    print("[OK] Plantilla generada: plantilla_tarifas_telefonia_importacion.xlsx")

if __name__ == "__main__":
    try:
        generar_plantilla()
    except Exception as e:
        print(f"[ERROR] {str(e)}")
        import sys
        sys.exit(1)
