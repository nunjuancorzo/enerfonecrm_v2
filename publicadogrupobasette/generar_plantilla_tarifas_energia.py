#!/usr/bin/env python3
"""
Script para generar plantilla de importación de tarifas de energía

Uso: 
  python3 generar_plantilla_tarifas_energia.py
"""

import xlsxwriter
from datetime import datetime

def crear_plantilla_tarifas_energia():
    """Crea la plantilla Excel para importar tarifas de energía"""
    
    # Crear libro de Excel
    workbook = xlsxwriter.Workbook('plantilla_tarifas_energia_importacion.xlsx')
    
    # Formatos
    header_format = workbook.add_format({
        'bold': True,
        'bg_color': '#4CAF50',
        'font_color': 'white',
        'border': 1,
        'align': 'center',
        'valign': 'vcenter'
    })
    
    ejemplo_format = workbook.add_format({
        'bg_color': '#E8F5E9',
        'border': 1
    })
    
    # Crear hoja principal
    worksheet = workbook.add_worksheet('Tarifas Energía')
    
    # Definir encabezados (igual que el Excel del usuario)
    headers = [
        'ID', 'COMERCIALIZADORA', 'TIPO', 'ENERGIA', 'TARIFA', 'PEAJE LUZ', 'PEAJE GAS',
        'POTENCIA 1', 'POTENCIA 2', 'POTENCIA 3', 'POTENCIA 4', 'POTENCIA 5', 'POTENCIA 6',
        'T. FIJO GAS', 'PVD SVA',
        'ENERGIA 1', 'ENERGIA 2', 'ENERGIA 3', 'ENERGIA 4', 'ENERGIA 5', 'ENERGIA 6',
        'T. VARIABLE GAS', 'DESCUENTO', 'OBJERVACIONES DESCUENTOS',
        'COMISION', 'PERMANENCIA', 'EXCEDENTES', 'BATERIA VIRTUAL', 'FECHA CARGA'
    ]
    
    # Escribir encabezados
    for col, header in enumerate(headers):
        worksheet.write(0, col, header, header_format)
    
    # Ajustar ancho de columnas
    worksheet.set_column(0, 0, 10)  # ID
    worksheet.set_column(1, 1, 20)  # COMERCIALIZADORA
    worksheet.set_column(2, 2, 15)  # TIPO
    worksheet.set_column(3, 3, 12)  # ENERGIA
    worksheet.set_column(4, 4, 30)  # TARIFA
    worksheet.set_column(5, 5, 12)  # PEAJE LUZ
    worksheet.set_column(6, 6, 12)  # PEAJE GAS
    worksheet.set_column(7, 12, 12)  # POTENCIAS
    worksheet.set_column(13, 14, 12)  # T. FIJO GAS, PVD SVA
    worksheet.set_column(15, 20, 12)  # ENERGIAS
    worksheet.set_column(21, 21, 15)  # T. VARIABLE GAS
    worksheet.set_column(22, 22, 12)  # DESCUENTO
    worksheet.set_column(23, 23, 30)  # OBSERVACIONES DESCUENTOS
    worksheet.set_column(24, 24, 12)  # COMISION
    worksheet.set_column(25, 25, 15)  # PERMANENCIA
    worksheet.set_column(26, 26, 12)  # EXCEDENTES
    worksheet.set_column(27, 27, 15)  # BATERIA VIRTUAL
    worksheet.set_column(28, 28, 20)  # FECHA CARGA
    
    # Datos de ejemplo
    ejemplos = [
        [
            '', 'NATURGY', 'RESIDENCIAL', 'LUZ', 'Tarifa Por Uso Luz', '2.0', '',
            0.110970, 0.033677, '', '', '', '',
            '', '',
            0.119900, 0.119900, 0.119900, '', '', '',
            '', '', '',
            70, 0, 0.07, 'SI', datetime.now().strftime('%Y-%m-%d')
        ],
        [
            '', 'IBERDROLA', 'PYMES', 'LUZ+GAS', 'Plan Estable', '2.0', 'RL3',
            0.115000, 0.035000, '', '', '', '',
            '3.50', '',
            0.125000, 0.120000, 0.085000, '', '', '',
            '0.050', '', 'Descuento especial primer año',
            65, '12M', 0.08, 'NO', datetime.now().strftime('%Y-%m-%d')
        ]
    ]
    
    for row_idx, ejemplo in enumerate(ejemplos):
        for col_idx, valor in enumerate(ejemplo):
            worksheet.write(row_idx + 1, col_idx, valor, ejemplo_format)
    
    # Crear hoja de instrucciones
    worksheet_inst = workbook.add_worksheet('Instrucciones')
    
    title_format = workbook.add_format({'bold': True, 'font_size': 14, 'font_color': '#2E7D32'})
    subtitle_format = workbook.add_format({'bold': True, 'font_color': '#2E7D32'})
    
    worksheet_inst.set_column(0, 0, 100)
    
    row = 0
    worksheet_inst.write(row, 0, 'INSTRUCCIONES - IMPORTACIÓN TARIFAS DE ENERGÍA', title_format)
    row += 2
    
    worksheet_inst.write(row, 0, 'Campos obligatorios:', subtitle_format)
    row += 1
    instrucciones = [
        '- COMERCIALIZADORA: Nombre de la comercializadora (NATURGY, IBERDROLA, ENDESA, etc.)',
        '- ENERGIA: Tipo de energía (LUZ, GAS, LUZ+GAS)',
        '- TARIFA: Nombre de la tarifa',
        '',
        'Campo especial:',
        '- ID: Dejar vacío para tarifas nuevas. Incluir el ID para actualizar tarifas existentes.',
        '',
        'Campos opcionales:', 
        '- TIPO: Tipo de cliente (Residencial, Pyme)',
        '- PEAJE LUZ: Tipo de peaje luz (2.0, 3.0, 6.1, 6.2)',
        '- PEAJE GAS: Tipo de peaje gas (RL1, RL2, RL3, RL4, RL5, RL6)',
        '- POTENCIA 1-6: Precios de potencia por periodo (valores decimales con 6 decimales)',
        '- ENERGIA 1-6: Precios de energía por periodo (valores decimales con 6 decimales)',
        '- T. FIJO GAS: Término fijo de gas (texto)',
        '- PVD SVA: Precio Vapor Diario SVA (texto)',
        '- T. VARIABLE GAS: Término variable de gas (texto)',
        '- DESCUENTO: Descuento aplicable (texto)',
        '- OBJERVACIONES DESCUENTOS: Observaciones sobre descuentos (texto)',
        '- COMISION: Comisión de la tarifa (valor decimal)',
        '- PERMANENCIA: Permanencia del contrato (0, 12M, 24M, etc.)',
        '- EXCEDENTES: Precio de excedentes (valor decimal)',
        '- BATERIA VIRTUAL: Si tiene batería virtual (SI/NO)',
        '- FECHA CARGA: Fecha de carga de la tarifa (formato YYYY-MM-DD)',
        '',
        'Notas importantes:',
        '- La primera fila DEBE contener los encabezados',
        '- Elimine las filas de ejemplo antes de importar sus datos',
        '- Los valores decimales deben usar punto (.) como separador (ej: 0.123)',
        '- Al menos debe tener POTENCIA 1 o ENERGIA 1 con valor',
        '- Las celdas vacías son válidas para campos opcionales',
        '- Asegúrese de que el nombre de la hoja es "Tarifas Energía"'
    ]
    
    for instruccion in instrucciones:
        worksheet_inst.write(row, 0, instruccion)
        row += 1
    
    workbook.close()
    print("[OK] Plantilla creada: plantilla_tarifas_energia_importacion.xlsx")

if __name__ == "__main__":
    try:
        crear_plantilla_tarifas_energia()
    except Exception as e:
        print(f"[ERROR] {str(e)}")
        import sys
        sys.exit(1)
