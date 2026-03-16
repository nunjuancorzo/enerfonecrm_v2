#!/usr/bin/env python3
"""
Script para importar tarifas de telefonía desde un archivo Excel a la base de datos MySQL

Lee automáticamente la base de datos desde appsettings.Production.json

Uso: python3 importar_tarifas_telefonia.py <archivo_excel>
"""

import sys
import os
import json
import re
import pandas as pd
import mysql.connector
from datetime import datetime
from mysql.connector import Error

def obtener_config_bd():
    """Lee la configuración de la base de datos desde appsettings.Production.json"""
    try:
        config_file = 'appsettings.Production.json'
        if not os.path.exists(config_file):
            print(f"[ERROR] Error: No se encuentra {config_file} en el directorio actual")
            print(f"   Directorio actual: {os.getcwd()}")
            print(f"   Ejecuta el script desde la carpeta donde está appsettings.Production.json")
            sys.exit(1)
        
        with open(config_file, 'r', encoding='utf-8') as f:
            config = json.load(f)
        
        connection_string = config.get('ConnectionStrings', {}).get('DefaultConnection', '')
        if not connection_string:
            print(f"[ERROR] Error: No se encontró ConnectionStrings.DefaultConnection en {config_file}")
            sys.exit(1)
        
        # Extraer información de la cadena de conexión
        db_match = re.search(r'Database=([^;]+)', connection_string)
        user_match = re.search(r'User=([^;]+)', connection_string)
        password_match = re.search(r'Password=([^;]+)', connection_string)
        host_match = re.search(r'Server=([^;]+)', connection_string)
        
        if not db_match:
            print(f"[ERROR] Error: No se pudo extraer el nombre de la base de datos")
            sys.exit(1)
        
        return {
            'host': host_match.group(1) if host_match else 'localhost',
            'database': db_match.group(1),
            'user': user_match.group(1) if user_match else 'root',
            'password': password_match.group(1) if password_match else ''
        }
    except Exception as e:
        print(f"[ERROR] Error al leer configuración: {str(e)}")
        sys.exit(1)

# Verificar argumentos
if len(sys.argv) < 2:
    print("Uso: python3 importar_tarifas_telefonia.py <archivo_excel>")
    print("\nNOTA: El script lee automáticamente la base de datos desde appsettings.Production.json")
    sys.exit(1)

archivo_excel_arg = sys.argv[1]

# Configuración de la base de datos (automática)
DB_CONFIG = obtener_config_bd()

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

def normalizar_operadora(nombre):
    """Normaliza el nombre de operadora aplicando Title Case"""
    if not nombre:
        return None
    # Aplicar Title Case: primera letra mayúscula, resto minúsculas
    nombre = nombre.strip()
    # Casos especiales
    if nombre.upper() == 'O2':
        return 'O2'
    if nombre.upper() == 'MASMOVIL':
        return 'Masmovil'
    return nombre.title()

def importar_tarifas_telefonia(archivo_excel):
    """Importa tarifas de telefonía desde Excel a MySQL. Si tiene columna ID, actualiza; si no, inserta"""
    
    result = {
        'importados': 0,
        'actualizados': 0,
        'errores': 0,
        'errores_detalle': []
    }
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel)
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        # Verificar si tiene columna ID  
        tiene_columna_id = 'ID' in df.columns
        if tiene_columna_id:
            print(f"[INFO] Columna ID detectada - Se actualizarán tarifas existentes")
        else:
            print(f"[INFO] Sin columna ID - Se insertarán tarifas nuevas")
        
        print(f"[OK] Se encontraron {len(df)} filas (incluyendo encabezados)")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        # Obtener operadoras válidas de la base de datos
        print("Obteniendo operadoras válidas...")
        cursor.execute("SELECT DISTINCT Nombre FROM operadoras")
        operadoras_validas = set(row[0] for row in cursor.fetchall())
        print(f"Operadoras válidas encontradas: {', '.join(sorted(operadoras_validas))}")
        
        # Definir valores válidos para otros campos
        tipos_validos = {'FibraMovil', 'Fibra', 'Movil', 'FibraMovilTV', 'FibraSegundaResidencia'}
        
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
                
                # Normalizar operadora
                operadora = normalizar_operadora(operadora)
                
                # Validar que la operadora existe en la base de datos
                if operadora not in operadoras_validas:
                    result['errores_detalle'].append(
                        f"Fila {fila_num}: La operadora '{operadora}' no existe en la base de datos. "
                        f"Operadoras válidas: {', '.join(sorted(operadoras_validas))}"
                    )
                    result['errores'] += 1
                    continue
                
                # Validar tipo
                if tipo not in tipos_validos:
                    result['errores_detalle'].append(
                        f"Fila {fila_num}: TIPO '{tipo}' no es válido. "
                        f"Valores válidos: {', '.join(sorted(tipos_validos))}"
                    )
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
                
                # Verificar si es una actualización o inserción
                tarifa_id = None
                es_actualizacion = False
                
                if tiene_columna_id:
                    tarifa_id_valor = row.get('ID')
                    if tarifa_id_valor and not pd.isna(tarifa_id_valor):
                        try:
                            tarifa_id = int(tarifa_id_valor)
                            # Verificar si existe
                            cursor.execute("SELECT id FROM tarifastelefonia WHERE id = %s", (tarifa_id,))
                            existe = cursor.fetchone()
                            es_actualizacion = existe is not None
                        except:
                            pass
                
                if es_actualizacion:
                    # UPDATE de tarifa existente
                    sql = """
                    UPDATE tarifastelefonia SET
                        compania = %s, tipo = %s, tarifa = %s, fibra = %s, gbmovil = %s, movil2 = %s,
                        tv1 = %s, tv2 = %s, precio = %s, comision = %s, precioNew = %s, comisionNew = %s,
                        permanencia = %s, fecha_carga = %s
                    WHERE id = %s
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
                        fecha_carga,
                        tarifa_id  # WHERE id
                    )
                    
                    cursor.execute(sql, valores)
                    result['actualizados'] += 1
                    
                else:
                    # INSERT nueva tarifa
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
        print(f"Nuevos: {result['importados']}")
        print(f"Actualizados: {result['actualizados']}")
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
