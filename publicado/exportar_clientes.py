#!/usr/bin/env python3
"""
Script para exportar todos los clientes existentes con ID a un archivo Excel
Los parámetros de conexión se leen desde appsettings.*.json si no se proporcionan

Uso: 
  python3 exportar_clientes.py
  python3 exportar_clientes.py <nombre_bd> [db_user] [db_password]
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
        print("Uso: python3 exportar_clientes.py [nombre_bd] [db_user] [db_password]")
        sys.exit(1)

# Configuración de la base de datos
DB_CONFIG = {
    'host': db_host,
    'database': database_name,
    'user': db_user,
    'password': db_password
}

def exportar_clientes():
    """Exporta todos los clientes con ID"""
    try:
        print(f"Conectando a la base de datos {database_name}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor(dictionary=True)
        
        # Consultar todos los clientes
        sql = """
        SELECT 
            id, tipo_cliente, nombre, dni_cif, cnae, dni_representante,
            email, telefono, tipo_via, direccion, numero, escalera,
            piso, puerta, aclarador, poblacion, provincia, codigo_postal,
            iban, representante, comercial, procedencia, observaciones
        FROM clientes_simple
        ORDER BY id
        """
        
        cursor.execute(sql)
        clientes = cursor.fetchall()
        
        print(f"[OK] Se encontraron {len(clientes)} clientes")
        
        if not clientes:
            print("[INFO] No hay clientes para exportar")
            return
        
        # Preparar datos para Excel
        datos_excel = []
        for cliente in clientes:
            datos_excel.append({
                'ID': cliente['id'],
                'TipoCliente': cliente['tipo_cliente'] or '',
                'Nombre': cliente['nombre'] or '',
                'DNI/CIF': cliente['dni_cif'] or '',
                'CNAE': cliente['cnae'] or '',
                'DNI Representante': cliente['dni_representante'] or '',
                'Email': cliente['email'] or '',
                'Teléfono': cliente['telefono'] or '',
                'TipoVia': cliente['tipo_via'] or '',
                'Dirección': cliente['direccion'] or '',
                'Número': cliente['numero'] or '',
                'Escalera': cliente['escalera'] or '',
                'Piso': cliente['piso'] or '',
                'Puerta': cliente['puerta'] or '',
                'Aclarador': cliente['aclarador'] or '',
                'Población': cliente['poblacion'] or '',
                'Provincia': cliente['provincia'] or '',
                'Código Postal': cliente['codigo_postal'] or '',
                'IBAN': cliente['iban'] or '',
                'Representante': cliente['representante'] or '',
                'Comercial': cliente['comercial'] or '',
                'Procedencia': cliente['procedencia'] or '',
                'Observaciones': cliente['observaciones'] or ''
            })
        
        # Crear DataFrame
        df = pd.DataFrame(datos_excel)
        
        # Crear DataFrame con valores válidos
        valores_validos = [
            ['CAMPO', 'VALORES VÁLIDOS', 'DESCRIPCIÓN'],
            ['', '', ''],
            ['TipoCliente*', '', ''],
            ['', 'Particular', 'Para clientes particulares/personas físicas'],
            ['', 'Autónomo', 'Para trabajadores autónomos'],
            ['', 'Pyme', 'Para pequeñas y medianas empresas'],
            ['', '', ''],
            ['TipoVia', '', ''],
            ['', 'Calle', ''],
            ['', 'Avenida', ''],
            ['', 'Paseo', ''],
            ['', 'Plaza', ''],
            ['', 'Carretera', ''],
            ['', 'Camino', ''],
            ['', 'Travesía', ''],
            ['', '', ''],
            ['NOTAS', '', ''],
            ['', '* Campos obligatorios', ''],
            ['', 'Si incluye columna ID, actualiza clientes existentes', ''],
            ['', 'Si no incluye ID, crea nuevos clientes', ''],
            ['', 'CNAE: Código de actividad económica (4 dígitos)', '']
        ]
        df_valores = pd.DataFrame(valores_validos)
        
        # Nombre del archivo con fecha actual
        fecha_actual = datetime.now().strftime('%Y%m%d_%H%M%S')
        nombre_archivo = f'clientes_exportados_{fecha_actual}.xlsx'
        
        # Exportar a Excel con múltiples hojas
        with pd.ExcelWriter(nombre_archivo, engine='openpyxl') as writer:
            df.to_excel(writer, index=False, sheet_name='Clientes')
            df_valores.to_excel(writer, index=False, sheet_name='Valores Válidos', header=False)
        
        print(f"[OK] Clientes exportados correctamente a: {nombre_archivo}")
        print(f"[OK] Total de clientes exportados: {len(datos_excel)}")
        
        cursor.close()
        conexion.close()
        
    except Error as e:
        print(f"[ERROR] Error de base de datos: {e}")
        sys.exit(1)
    except Exception as e:
        print(f"[ERROR] {e}")
        sys.exit(1)

if __name__ == "__main__":
    exportar_clientes()
