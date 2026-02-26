#!/usr/bin/env python3
"""
Script para importar tarifas de telefonía desde un archivo Excel a la base de datos MySQL
Uso: python3 importar_tarifas_telefonia.py <nombre_bd> tarifas_telefonia.xlsx
"""

import sys
import pandas as pd
import mysql.connector
from datetime import datetime
from mysql.connector import Error

# Verificar argumentos
if len(sys.argv) < 3:
    print("Uso: python3 importar_tarifas_telefonia.py <nombre_bd> <archivo_excel>")
    sys.exit(1)

database_name = sys.argv[1]
archivo_excel_arg = sys.argv[2]

# Configuración de la base de datos
DB_CONFIG = {
    'host': 'localhost',
    'database': database_name,
    'user': 'enerfone',
    'password': 'Salaiet6680.'  # Credenciales de producción
}

def limpiar_valor(valor):
    """Limpia valores None, NaN o vacíos"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    return str(valor).strip()

def limpiar_decimal(valor):
    """Limpia y convierte valores decimales"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    valor_str = str(valor).strip()
    return valor_str

def limpiar_fecha(valor):
    """Limpia y convierte valores de fecha"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    
    # Si ya es datetime, convertir a string con formato SQL
    if isinstance(valor, datetime):
        return valor.strftime('%Y-%m-%d %H:%M:%S')
    
    # Si es un string que no es una fecha válida, devolver None
    valor_str = str(valor).strip().upper()
    if 'SELLO' in valor_str or 'FECHA Y HORA' in valor_str or valor_str == 'FECHA CARGA':
        return None
    
    # Intentar parsear como fecha
    try:
        from dateutil import parser
        fecha = parser.parse(str(valor))
        return fecha.strftime('%Y-%m-%d %H:%M:%S')
    except:
        # Si no se puede parsear, devolver None
        return None

def importar_tarifas_telefonia(archivo_excel):
    """Importa tarifas de telefonía desde Excel a MySQL"""
    
    result = {
        'importados': 0,
        'errores': 0,
        'errores_detalle': []
    }
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel)
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"[OK] Se encontraron {len(df)} filas (incluyendo encabezados)")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        # Procesar cada fila
        for index, row in df.iterrows():
            fila_num = index + 2  # +2 porque index empieza en 0 y hay encabezado
            
            try:
                # Leer campos del Excel
                operadora = limpiar_valor(row.get('OPERADORA'))
                tipo = limpiar_valor(row.get('TIPO'))
                tarifa = limpiar_valor(row.get('TARIFA'))
                fibra = limpiar_valor(row.get('FIBRA'))
                movil1 = limpiar_valor(row.get('MOVIL 1'))
                movil2 = limpiar_valor(row.get('MOVIL 2'))
                tv1 = limpiar_valor(row.get('TV1'))
                tv2 = limpiar_valor(row.get('TV2'))
                precio = limpiar_decimal(row.get('PRECIO'))
                comision = limpiar_decimal(row.get('COMISION'))
                permanencia = limpiar_valor(row.get('PERMANENCIA'))
                fecha_carga = limpiar_fecha(row.get('FECHA CARGA'))
                
                # Saltar filas vacías
                if not operadora and not tarifa:
                    continue
                
                # Validaciones básicas
                if not operadora:
                    result['errores_detalle'].append(f"Fila {fila_num}: OPERADORA es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not tipo:
                    result['errores_detalle'].append(f"Fila {fila_num}: TIPO es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Calcular precio y comisión numéricos
                precio_new = 0
                comision_new = 0
                
                if precio:
                    try:
                        # Intentar convertir precio a decimal
                        precio_limpio = precio.replace(',', '.').replace('€', '').strip()
                        precio_new = float(precio_limpio)
                    except:
                        pass
                
                if comision:
                    try:
                        # Intentar convertir comisión a decimal
                        comision_limpia = comision.replace(',', '.').replace('€', '').strip()
                        comision_new = float(comision_limpia)
                    except:
                        pass
                
                # Preparar valores para inserción
                sql = """
                INSERT INTO tarifastelefonia (
                    compania, tipo, tarifa, fibra, gbmovil, movil2,
                    tv1, tv2, precio, comision, precioNew, comisionNew,
                    permanencia, fecha_carga
                ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
                """
                
                valores = (
                    operadora,
                    tipo,
                    tarifa,
                    fibra,
                    movil1,
                    movil2,
                    tv1,
                    tv2,
                    precio,
                    comision,
                    precio_new,
                    comision_new,
                    permanencia,
                    fecha_carga
                )
                
                cursor.execute(sql, valores)
                result['importados'] += 1
                
            except Error as e:
                error_msg = f"Fila {fila_num}: {e.errno} ({e.sqlstate}): {e.msg}"
                result['errores_detalle'].append(error_msg)
                result['errores'] += 1
                print(f"[ERROR] {error_msg}")
                continue
            except Exception as e:
                error_msg = f"Fila {fila_num}: Error inesperado - {str(e)}"
                result['errores_detalle'].append(error_msg)
                result['errores'] += 1
                print(f"[ERROR] {error_msg}")
                continue
        
        # Confirmar cambios
        conexion.commit()
        print(f"[OK] Importación completada")
        print(f"Importados: {result['importados']}")
        print(f"Errores: {result['errores']}")
        
        # Cerrar conexión
        cursor.close()
        conexion.close()
        
    except FileNotFoundError:
        result['errores_detalle'].append(f"No se encontró el archivo: {archivo_excel}")
        print(f"[ERROR] No se encontró el archivo: {archivo_excel}")
    except Error as e:
        result['errores_detalle'].append(f"Error de base dedatos: {e.errno} ({e.sqlstate}): {e.msg}")
        print(f"[ERROR] Error de base de datos: {e}")
    except Exception as e:
        result['errores_detalle'].append(f"Error inesperado: {str(e)}")
        print(f"[ERROR] Error inesperado: {e}")
    
    return result

if __name__ == "__main__":
    resultado = importar_tarifas_telefonia(archivo_excel_arg)
    
    # Imprimir resultado para que lo capture el programa
    print(f"Importados: {resultado['importados']}")
    print(f"Errores: {resultado['errores']}")
    
    if resultado['errores'] > 0:
        print("\nDetalles de errores:")
        for error in resultado['errores_detalle']:
            print(f"- {error}")
