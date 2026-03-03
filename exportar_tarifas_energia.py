#!/usr/bin/env python3
"""
Script para exportar todas las tarifas de energía existentes con ID a un archivo Excel
Uso: python3 exportar_tarifas_energia.py <nombre_bd> [db_user] [db_password]
"""

import sys
import pandas as pd
import mysql.connector
from mysql.connector import Error
from datetime import datetime

# Verificar argumentos
if len(sys.argv) < 2:
    print("Uso: python3 exportar_tarifas_energia.py <nombre_bd> [db_user] [db_password]")
    sys.exit(1)

database_name = sys.argv[1]
db_user = sys.argv[2] if len(sys.argv) > 2 else 'root'
db_password = sys.argv[3] if len(sys.argv) > 3 else 'A76262136.r'

# Configuración de la base de datos
DB_CONFIG = {
    'host': 'localhost',
    'database': database_name,
    'user': db_user,
    'password': db_password
}

def formatear_decimal_con_coma(valor):
    """Formatea valores decimales con coma"""
    if valor is None or pd.isna(valor):
        return ''
    try:
        # Si es string, convertir a float
        if isinstance(valor, str):
            valor = valor.replace(',', '.')
            valor = float(valor)
        # Formatear con 6 decimales y reemplazar punto por coma
        return str(valor).replace('.', ',')
    except:
        return str(valor) if valor else ''

def exportar_tarifas_energia():
    """Exporta todas las tarifas de energía con ID"""
    try:
        print(f"Conectando a la base de datos {database_name}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor(dictionary=True)
        
        # Consultar todas las tarifas
        sql = """
        SELECT 
            id, empresa, tipo, nombre,
            potencia1, potencia2, potencia3, potencia4, potencia5, potencia6,
            energia1, energia2, energia3, energia4, energia5, energia6,
            tipo_cliente, peaje, peaje_gas, termino_fijo_gas, pvd_sva, termino_variable_gas,
            descuento, observaciones_descuentos, comision, permanencia, excedentes,
            bateria_virtual, fecha_carga
        FROM tarifasenergia
        ORDER BY id
        """
        
        cursor.execute(sql)
        tarifas = cursor.fetchall()
        
        print(f"[OK] Se encontraron {len(tarifas)} tarifas")
        
        if not tarifas:
            print("[INFO] No hay tarifas para exportar")
            return
        
        # Preparar datos para Excel
        datos_excel = []
        for tarifa in tarifas:
            datos_excel.append({
                'ID': tarifa['id'],
                'COMERCIALIZADORA': tarifa['empresa'] or '',
                'TIPO': tarifa['tipo_cliente'] or '',
                'ENERGIA': tarifa['tipo'] or '',
                'TARIFA': tarifa['nombre'] or '',
                'PEAJE LUZ': tarifa['peaje'] or '',
                'PEAJE GAS': tarifa['peaje_gas'] or '',
                'POTENCIA 1': formatear_decimal_con_coma(tarifa['potencia1']),
                'POTENCIA 2': formatear_decimal_con_coma(tarifa['potencia2']),
                'POTENCIA 3': formatear_decimal_con_coma(tarifa['potencia3']),
                'POTENCIA 4': formatear_decimal_con_coma(tarifa['potencia4']),
                'POTENCIA 5': formatear_decimal_con_coma(tarifa['potencia5']),
                'POTENCIA 6': formatear_decimal_con_coma(tarifa['potencia6']),
                'T. FIJO GAS': tarifa['termino_fijo_gas'] or '',
                'PVD SVA': tarifa['pvd_sva'] or '',
                'ENERGIA 1': formatear_decimal_con_coma(tarifa['energia1']),
                'ENERGIA 2': formatear_decimal_con_coma(tarifa['energia2']),
                'ENERGIA 3': formatear_decimal_con_coma(tarifa['energia3']),
                'ENERGIA 4': formatear_decimal_con_coma(tarifa['energia4']),
                'ENERGIA 5': formatear_decimal_con_coma(tarifa['energia5']),
                'ENERGIA 6': formatear_decimal_con_coma(tarifa['energia6']),
                'T. VARIABLE GAS': tarifa['termino_variable_gas'] or '',
                'DESCUENTO': tarifa['descuento'] or '',
                'OBJERVACIONES DESCUENTOS': tarifa['observaciones_descuentos'] or '',
                'COMISION': formatear_decimal_con_coma(tarifa['comision']),
                'PERMANENCIA': tarifa['permanencia'] or '',
                'EXCEDENTES': formatear_decimal_con_coma(tarifa['excedentes']),
                'BATERIA VIRTUAL': tarifa['bateria_virtual'] or '',
                'FECHA CARGA': tarifa['fecha_carga'].strftime('%Y-%m-%d') if tarifa['fecha_carga'] else ''
            })
        
        # Crear DataFrame
        df = pd.DataFrame(datos_excel)
        
        # Nombre del archivo con timestamp
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        nombre_archivo = f'tarifas_energia_exportacion_{timestamp}.xlsx'
        
        # Exportar a Excel
        df.to_excel(nombre_archivo, index=False, sheet_name='Tarifas Energía')
        
        print(f"[OK] Tarifas exportadas correctamente a: {nombre_archivo}")
        print(f"[OK] Total de tarifas exportadas: {len(datos_excel)}")
        
        cursor.close()
        conexion.close()
        
    except Error as e:
        print(f"[ERROR] Error de base de datos: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"[ERROR] Error inesperado: {e}")
        sys.exit(1)

if __name__ == "__main__":
    exportar_tarifas_energia()
