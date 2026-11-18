#!/usr/bin/env python3
"""
Script para importar tarifas de energía desde archivos Excel a la base de datos MySQL

Lee automáticamente la base de datos desde appsettings.Production.json

Uso: 
  python3 importar_tarifas_energia.py <archivo_excel>
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
    print("Uso: python3 importar_tarifas_energia.py <archivo_excel>")
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

def normalizar_comercializadora(nombre):
    """Normaliza el nombre de la comercializadora a Title Case"""
    if not nombre:
        return nombre
    # Convertir a Title Case (primera letra de cada palabra en mayúscula)
    return nombre.strip().title()

def limpiar_decimal(valor):
    """Limpia y convierte valores decimales"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    try:
        # Reemplazar coma por punto para decimales
        valor_str = str(valor).replace(',', '.')
        return float(valor_str)
    except:
        return None

def formatear_decimal_6(valor):
    """Formatea un valor decimal con 6 decimales y coma como separador"""
    if valor is None:
        return None
    try:
        # Formatear con 6 decimales
        return f"{valor:.6f}".replace('.', ',')
    except:
        return None

def limpiar_fecha(valor):
    """Limpia y convierte valores de fecha"""
    if pd.isna(valor) or valor == '' or valor is None:
        return datetime.now()
    try:
        if isinstance(valor, datetime):
            return valor
        return datetime.strptime(str(valor), '%Y-%m-%d')
    except:
        return datetime.now()

def importar_tarifas_energia(archivo_excel):
    """Importa tarifas de energía desde un archivo Excel. Si tiene columna ID, actualiza; si no, inserta"""
    result = {'importados': 0, 'actualizados': 0, 'errores': 0, 'errores_detalle': []}
    
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
        
        # Obtener comercializadoras válidas
        cursor.execute("SELECT nombre FROM comercializadoras")
        comercializadoras_validas = {normalizar_comercializadora(row[0]) for row in cursor.fetchall()}
        print(f"[OK] Comercializadoras válidas: {', '.join(sorted(comercializadoras_validas))}")
        
        # Valores válidos para campos con opciones limitadas
        tipos_energia_validos = {'LUZ', 'GAS', 'LUZ+GAS'}
        tipos_cliente_validos = {'Residencial', 'Pyme'}
        peajes_luz_validos = {'2.0', '3.0', '6.1', '6.2'}
        peajes_gas_validos = {'RL1', 'RL2', 'RL3', 'RL4', 'RL5', 'RL6'}
        
        # Procesar cada fila
        for index, row in df.iterrows():
            fila_num = index + 2  # +2 porque index empieza en 0 y hay encabezado
            
            try:
                # Leer campos del Excel
                comercializadora = limpiar_valor(row.get('COMERCIALIZADORA'))
                tipo_cliente = limpiar_valor(row.get('TIPO'))
                tipo_energia = limpiar_valor(row.get('ENERGIA'))
                tarifa = limpiar_valor(row.get('TARIFA'))
                peaje = limpiar_valor(row.get('PEAJE LUZ'))
                peaje_gas = limpiar_valor(row.get('PEAJE GAS'))
                
                # Saltar filas vacías
                if not comercializadora and not tarifa:
                    continue
                
                # Normalizar nombre de comercializadora
                if comercializadora:
                    comercializadora = normalizar_comercializadora(comercializadora)
                
                # Validaciones básicas
                if not comercializadora:
                    result['errores_detalle'].append(f"Fila {fila_num}: COMERCIALIZADORA es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Validar que la comercializadora existe
                if comercializadora not in comercializadoras_validas:
                    result['errores_detalle'].append(
                        f"Fila {fila_num}: COMERCIALIZADORA '{comercializadora}' no existe en el sistema. "
                        f"Valores válidos: {', '.join(sorted(comercializadoras_validas))}"
                    )
                    result['errores'] += 1
                    continue
                
                if not tipo_energia:
                    result['errores_detalle'].append(f"Fila {fila_num}: ENERGIA es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Validar tipo de energía
                tipo_energia_normalizado = tipo_energia.upper().strip()
                if tipo_energia_normalizado not in tipos_energia_validos:
                    result['errores_detalle'].append(
                        f"Fila {fila_num}: ENERGIA '{tipo_energia}' no es válido. "
                        f"Valores válidos: {', '.join(sorted(tipos_energia_validos))}"
                    )
                    result['errores'] += 1
                    continue
                tipo_energia = tipo_energia_normalizado
                
                # Validar tipo de cliente si está presente
                if tipo_cliente:
                    tipo_cliente_normalizado = tipo_cliente.strip().title()
                    if tipo_cliente_normalizado not in tipos_cliente_validos:
                        result['errores_detalle'].append(
                            f"Fila {fila_num}: TIPO '{tipo_cliente}' no es válido. "
                            f"Valores válidos: {', '.join(sorted(tipos_cliente_validos))}"
                        )
                        result['errores'] += 1
                        continue
                    tipo_cliente = tipo_cliente_normalizado
                
                # Validar peaje luz si está presente
                if peaje:
                    peaje = peaje.strip()
                    if peaje not in peajes_luz_validos:
                        result['errores_detalle'].append(
                            f"Fila {fila_num}: PEAJE '{peaje}' no es válido. "
                            f"Valores válidos: {', '.join(sorted(peajes_luz_validos))}"
                        )
                        result['errores'] += 1
                        continue
                
                # Validar peaje gas si está presente
                if peaje_gas:
                    peaje_gas = peaje_gas.strip().upper()
                    if peaje_gas not in peajes_gas_validos:
                        result['errores_detalle'].append(
                            f"Fila {fila_num}: PEAJE GAS '{peaje_gas}' no es válido. "
                            f"Valores válidos: {', '.join(sorted(peajes_gas_validos))}"
                        )
                        result['errores'] += 1
                        continue
                
                if not tarifa:
                    result['errores_detalle'].append(f"Fila {fila_num}: TARIFA es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Potencias
                potencia1 = limpiar_decimal(row.get('POTENCIA 1'))
                potencia2 = limpiar_decimal(row.get('POTENCIA 2'))
                potencia3 = limpiar_decimal(row.get('POTENCIA 3'))
                potencia4 = limpiar_decimal(row.get('POTENCIA 4'))
                potencia5 = limpiar_decimal(row.get('POTENCIA 5'))
                potencia6 = limpiar_decimal(row.get('POTENCIA 6'))
                
                # Energías
                energia1 = limpiar_decimal(row.get('ENERGIA 1'))
                energia2 = limpiar_decimal(row.get('ENERGIA 2'))
                energia3 = limpiar_decimal(row.get('ENERGIA 3'))
                energia4 = limpiar_decimal(row.get('ENERGIA 4'))
                energia5 = limpiar_decimal(row.get('ENERGIA 5'))
                energia6 = limpiar_decimal(row.get('ENERGIA 6'))
                
                # Otros campos
                termino_fijo_gas = limpiar_valor(row.get('T. FIJO GAS'))
                pvd_sva = limpiar_valor(row.get('PVD SVA'))
                termino_variable_gas = limpiar_valor(row.get('T. VARIABLE GAS'))
                descuento = limpiar_valor(row.get('DESCUENTO'))
                observaciones_descuentos = limpiar_valor(row.get('OBJERVACIONES DESCUENTOS'))
                comision = limpiar_decimal(row.get('COMISION'))
                permanencia = limpiar_valor(row.get('PERMANENCIA'))
                excedentes = limpiar_decimal(row.get('EXCEDENTES'))
                bateria_virtual = limpiar_valor(row.get('BATERIA VIRTUAL'))
                fecha_carga = limpiar_fecha(row.get('FECHA CARGA'))
                
                # Calcular precio estimado (provisional - puede ajustarse)
                # Por ahora usamos un valor base o 0
                precio_new = comision if comision else 0
                
                # Verificar si es una actualización o inserción
                tarifa_id = None
                es_actualizacion = False
                
                if tiene_columna_id:
                    tarifa_id_valor = row.get('ID')
                    if tarifa_id_valor and not pd.isna(tarifa_id_valor):
                        try:
                            tarifa_id = int(tarifa_id_valor)
                            # Verificar si existe
                            cursor.execute("SELECT id FROM tarifasenergia WHERE id = %s", (tarifa_id,))
                            existe = cursor.fetchone()
                            es_actualizacion = existe is not None
                        except:
                            pass
                
                if es_actualizacion:
                    # UPDATE de tarifa existente
                    sql = """
                    UPDATE tarifasenergia SET
                        empresa = %s, tipo = %s, nombre = %s, 
                        potencia1 = %s, potencia2 = %s, potencia3 = %s, potencia4 = %s, potencia5 = %s, potencia6 = %s,
                        energia1 = %s, energia2 = %s, energia3 = %s, energia4 = %s, energia5 = %s, energia6 = %s,
                        comision = %s, precioNew = %s,
                        tipo_cliente = %s, peaje = %s, peaje_gas = %s, termino_fijo_gas = %s, pvd_sva = %s, termino_variable_gas = %s,
                        descuento = %s, observaciones_descuentos = %s, permanencia = %s, excedentes = %s,
                        bateria_virtual = %s, fecha_carga = %s,
                        termino_fijo_diario = %s, precio_potencia_p1 = %s, precio_potencia_p2 = %s, precio_potencia_p3 = %s,
                        precio_energia_p1 = %s, precio_energia_p2 = %s, precio_energia_p3 = %s
                    WHERE id = %s
                    """
                    
                    valores = (
                        comercializadora, tipo_energia, tarifa,
                        formatear_decimal_6(potencia1),
                        formatear_decimal_6(potencia2),
                        formatear_decimal_6(potencia3),
                        formatear_decimal_6(potencia4),
                        formatear_decimal_6(potencia5),
                        formatear_decimal_6(potencia6),
                        formatear_decimal_6(energia1),
                        formatear_decimal_6(energia2),
                        formatear_decimal_6(energia3),
                        formatear_decimal_6(energia4),
                        formatear_decimal_6(energia5),
                        formatear_decimal_6(energia6),
                        comision if comision else 0,
                        precio_new,
                        tipo_cliente, peaje, peaje_gas, termino_fijo_gas, pvd_sva, termino_variable_gas,
                        descuento, observaciones_descuentos, permanencia, 
                        str(excedentes) if excedentes else None,
                        bateria_virtual, fecha_carga,
                        None,  # termino_fijo_diario (calculable si es necesario)
                        potencia1,  # precio_potencia_p1
                        potencia2,  # precio_potencia_p2
                        potencia3,  # precio_potencia_p3
                        energia1,   # precio_energia_p1
                        energia2,   # precio_energia_p2
                        energia3,   # precio_energia_p3
                        tarifa_id   # WHERE id
                    )
                    
                    cursor.execute(sql, valores)
                    result['actualizados'] += 1
                    
                else:
                    # INSERT nueva tarifa
                    sql = """
                    INSERT INTO tarifasenergia (
                        empresa, tipo, nombre, 
                        potencia1, potencia2, potencia3, potencia4, potencia5, potencia6,
                        energia1, energia2, energia3, energia4, energia5, energia6,
                        comision, precioNew,
                        tipo_cliente, peaje, peaje_gas, termino_fijo_gas, pvd_sva, termino_variable_gas,
                        descuento, observaciones_descuentos, permanencia, excedentes,
                        bateria_virtual, fecha_carga,
                        termino_fijo_diario, precio_potencia_p1, precio_potencia_p2, precio_potencia_p3,
                        precio_energia_p1, precio_energia_p2, precio_energia_p3
                    ) VALUES (
                        %s, %s, %s,
                        %s, %s, %s, %s, %s, %s,
                        %s, %s, %s, %s, %s, %s,
                        %s, %s,
                        %s, %s, %s, %s, %s, %s,
                        %s, %s, %s, %s,
                        %s, %s,
                        %s, %s, %s, %s,
                        %s, %s, %s
                    )
                    """
                    
                    valores = (
                        comercializadora, tipo_energia, tarifa,
                        formatear_decimal_6(potencia1),
                        formatear_decimal_6(potencia2),
                        formatear_decimal_6(potencia3),
                        formatear_decimal_6(potencia4),
                        formatear_decimal_6(potencia5),
                        formatear_decimal_6(potencia6),
                        formatear_decimal_6(energia1),
                        formatear_decimal_6(energia2),
                        formatear_decimal_6(energia3),
                        formatear_decimal_6(energia4),
                        formatear_decimal_6(energia5),
                        formatear_decimal_6(energia6),
                        comision if comision else 0,
                        precio_new,
                        tipo_cliente, peaje, peaje_gas, termino_fijo_gas, pvd_sva, termino_variable_gas,
                        descuento, observaciones_descuentos, permanencia, 
                        str(excedentes) if excedentes else None,
                        bateria_virtual, fecha_carga,
                        None,  # termino_fijo_diario (calculable si es necesario)
                        potencia1,  # precio_potencia_p1
                        potencia2,  # precio_potencia_p2
                        potencia3,  # precio_potencia_p3
                        energia1,   # precio_energia_p1
                        energia2,   # precio_energia_p2
                        energia3    # precio_energia_p3
                    )
                    
                    cursor.execute(sql, valores)
                    result['importados'] += 1
                
                if (result['importados'] + result['actualizados']) % 10 == 0:
                    print(f"  Procesadas {result['importados'] + result['actualizados']} tarifas...")
                
            except Exception as e:
                result['errores'] += 1
                result['errores_detalle'].append(f"Fila {fila_num}: {str(e)}")
                print(f"[ERROR] Fila {fila_num}: {str(e)}")
        
        # Confirmar cambios
        conexion.commit()
        cursor.close()
        conexion.close()
        
        print(f"\n[OK] Importación completada")
        print(f"  Nuevos: {result['importados']}")
        print(f"  Actualizados: {result['actualizados']}")
        print(f"  Errores: {result['errores']}")
        
    except FileNotFoundError:
        result['errores_detalle'].append(f"No se encontró el archivo: {archivo_excel}")
        print(f"[ERROR] No se encontró el archivo: {archivo_excel}")
    except Error as e:
        result['errores_detalle'].append(f"Error de base de datos: {str(e)}")
        print(f"[ERROR] Error de base de datos: {str(e)}")
    except Exception as e:
        result['errores_detalle'].append(f"Error inesperado: {str(e)}")
        print(f"[ERROR] Error inesperado: {str(e)}")
    
    return result

if __name__ == "__main__":
    resultado = importar_tarifas_energia(archivo_excel_arg)
    
    # Imprimir resultado en formato parseable
    print(f"\nImportados: {resultado['importados']}")
    print(f"Errores: {resultado['errores']}")
    
    for error in resultado['errores_detalle']:
        print(f"[ERROR] {error}")
