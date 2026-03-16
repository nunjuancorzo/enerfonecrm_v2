#!/usr/bin/env python3
"""
Script para exportar todos los servicios existentes con ID a un archivo Excel
Los parámetros de conexión se leen desde appsettings.*.json si no se proporcionan

Uso: 
  python3 exportar_servicios.py
  python3 exportar_servicios.py <nombre_bd> [db_user] [db_password]
"""

import sys
import os
import json
import re
import pandas as pd
import mysql.connector
from mysql.connector import Error
from datetime import datetime

def obtener_config_bd():
    """Lee la configuración de la base de datos desde appsettings.*.json"""
    try:
        # Buscar archivos de configuración en orden de prioridad
        # PRODUCCION PRIMERO para que funcione correctamente en servidores
        config_files = [
            'appsettings.Production.json',
            'appsettings.Production.Enerfone.json',
            'appsettings.Production.GrupoBasette.json',
            'EnerfoneCRM/appsettings.Production.json',
            'appsettings.json',
            'EnerfoneCRM/appsettings.json',
            'appsettings.Development.json',
            'EnerfoneCRM/appsettings.Development.json'
        ]
        
        config_file = None
        for file_path in config_files:
            if os.path.exists(file_path):
                config_file = file_path
                print(f"[CONFIG] Usando archivo de configuracion: {config_file}")
                break
        
        if not config_file:
            print(f"[ERROR] No se encuentra ningun archivo de configuracion")
            print(f"   Directorio actual: {os.getcwd()}")
            return None
        
        with open(config_file, 'r', encoding='utf-8') as f:
            config = json.load(f)
        
        connection_string = config.get('ConnectionStrings', {}).get('DefaultConnection', '')
        if not connection_string:
            return None
        
        # Extraer información de la cadena de conexión
        db_match = re.search(r'Database=([^;]+)', connection_string)
        user_match = re.search(r'User=([^;]+)', connection_string)
        password_match = re.search(r'Password=([^;]+)', connection_string)
        host_match = re.search(r'Server=([^;]+)', connection_string)
        
        if not db_match:
            return None
        
        database_name = db_match.group(1)
        db_user = user_match.group(1) if user_match else 'root'
        db_password = password_match.group(1) if password_match else ''
        db_host = host_match.group(1) if host_match else 'localhost'
        
        print(f"[DB] Conectando a: {db_host}/{database_name} como {db_user}")
        
        return {
            'host': db_host,
            'database': database_name,
            'user': db_user,
            'password': db_password
        }
    except Exception as e:
        print(f"[ERROR] Error al leer configuracion: {str(e)}")
        return None

# Intentar obtener configuración desde archivo o usar parámetros
if len(sys.argv) >= 2:
    # Usar parámetros de línea de comandos
    database_name = sys.argv[1]
    db_user = sys.argv[2] if len(sys.argv) > 2 else 'root'
    db_password = sys.argv[3] if len(sys.argv) > 3 else 'A76262136.r'
    db_host = 'localhost'
    print(f"[PARAMS] Usando parametros de linea de comandos")
else:
    # Intentar leer desde archivo de configuración
    config = obtener_config_bd()
    if config:
        database_name = config['database']
        db_user = config['user']
        db_password = config['password']
        db_host = config['host']
    else:
        print("Uso: python3 exportar_servicios.py [nombre_bd] [db_user] [db_password]")
        sys.exit(1)

# Configuración de la base de datos
DB_CONFIG = {
    'host': db_host,
    'database': database_name,
    'user': db_user,
    'password': db_password
}

def exportar_servicios():
    """Exporta todos los servicios con ID"""
    try:
        print(f"Conectando a la base de datos {database_name}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor(dictionary=True)
        
        # Consultar todos los servicios
        sql = """
        SELECT 
            id, tipo, nombreServicio, precio, empresa
        FROM servicios
        ORDER BY id
        """
        
        cursor.execute(sql)
        servicios = cursor.fetchall()
        
        print(f"[OK] Se encontraron {len(servicios)} servicios")
        
        if not servicios:
            print("[INFO] No hay servicios para exportar")
            return
        
        # Preparar datos para Excel
        datos_excel = []
        for servicio in servicios:
            # Formatear precio con coma como separador decimal
            precio = servicio['precio'] or ''
            if precio:
                try:
                    # Convertir a decimal y formatear con coma
                    precio_formateado = str(float(precio)).replace('.', ',')
                except:
                    precio_formateado = str(precio).replace('.', ',')
            else:
                precio_formateado = ''
            
            datos_excel.append({
                'ID': servicio['id'],
                'Tipo': servicio['tipo'] or '',
                'NombreServicio': servicio['nombreServicio'] or '',
                'Precio': precio_formateado,
                'Empresa': servicio['empresa'] or ''
            })
        
        # Crear DataFrame
        df = pd.DataFrame(datos_excel)
        
        # Nombre del archivo con fecha actual
        fecha_actual = datetime.now().strftime('%Y%m%d_%H%M%S')
        nombre_archivo = f'servicios_exportados_{fecha_actual}.xlsx'
        
        # Exportar a Excel (sin xlsxwriter, usando openpyxl por defecto)
        df.to_excel(nombre_archivo, index=False, sheet_name='Servicios')
        
        print(f"[OK] Servicios exportados correctamente a: {nombre_archivo}")
        print(f"[OK] Total de servicios exportados: {len(datos_excel)}")
        
        cursor.close()
        conexion.close()
        
    except Error as e:
        print(f"[ERROR] Error de base de datos: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"[ERROR] {e}")
        sys.exit(1)

if __name__ == "__main__":
    exportar_servicios()
