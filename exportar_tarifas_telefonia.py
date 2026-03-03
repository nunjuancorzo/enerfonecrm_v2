#!/usr/bin/env python3
"""
Script para exportar todas las tarifas de telefonía existentes con ID a un archivo Excel
Uso: python3 exportar_tarifas_telefonia.py <nombre_bd> [db_user] [db_password]
"""

import sys
import pandas as pd
import mysql.connector
from mysql.connector import Error
from datetime import datetime

# Verificar argumentos
if len(sys.argv) < 2:
    print("Uso: python3 exportar_tarifas_telefonia.py <nombre_bd> [db_user] [db_password]")
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

def exportar_tarifas_telefonia():
    """Exporta todas las tarifas de telefonía con ID"""
    try:
        print(f"Conectando a la base de datos {database_name}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor(dictionary=True)
        
        # Consultar todas las tarifas
        sql = """
        SELECT 
            id, compania, tipo, tarifa, fibra, gbmovil, movil2,
            tv1, tv2, precio, comision, permanencia, fecha_carga
        FROM tarifastelefonia
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
                'OPERADORA': tarifa['compania'] or '',
                'TIPO': tarifa['tipo'] or '',
                'TARIFA': tarifa['tarifa'] or '',
                'FIBRA': tarifa['fibra'] or '',
                'MOVIL 1': tarifa['gbmovil'] or '',
                'MOVIL 2': tarifa['movil2'] or '',
                'TV1': tarifa['tv1'] or '',
                'TV2': tarifa['tv2'] or '',
                'PRECIO': tarifa['precio'] or '',
                'COMISION': tarifa['comision'] or '',
                'PERMANENCIA': tarifa['permanencia'] or '',
                'FECHA CARGA': tarifa['fecha_carga'].strftime('%Y-%m-%d') if tarifa['fecha_carga'] else ''
            })
        
        # Crear DataFrame
        df = pd.DataFrame(datos_excel)
        
        # Nombre del archivo con timestamp
        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
        nombre_archivo = f'tarifas_telefonia_exportacion_{timestamp}.xlsx'
        
        # Exportar a Excel
        df.to_excel(nombre_archivo, index=False, sheet_name='Tarifas Telefonía')
        
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
    exportar_tarifas_telefonia()
