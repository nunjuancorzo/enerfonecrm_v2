#!/usr/bin/env python3
"""
Script para importar tarifas de energía desde archivos Excel a la base de datos MySQL

Uso: 
  python3 importar_tarifas_energia.py <nombre_bd> tarifas_energia.xlsx
"""

import sys
import pandas as pd
import mysql.connector
from datetime import datetime
from mysql.connector import Error

# Verificar argumentos
if len(sys.argv) < 3:
    print("Uso: python3 importar_tarifas_energia.py <nombre_bd> <archivo_excel>")
    sys.exit(1)

database_name = sys.argv[1]
archivo_excel_arg = sys.argv[2]

# Configuración de la base de datos
DB_CONFIG = {
    'host': 'localhost',
    'database': database_name,
    'user': 'root',
    'password': 'A76262136.r'  # Añadir contraseña si es necesario
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
    try:
        # Reemplazar coma por punto para decimales
        valor_str = str(valor).replace(',', '.')
        return float(valor_str)
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
    """Importa tarifas de energía desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
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
                comercializadora = limpiar_valor(row.get('COMERCIALIZADORA'))
                tipo_cliente = limpiar_valor(row.get('TIPO'))
                tipo_energia = limpiar_valor(row.get('ENERGIA'))
                tarifa = limpiar_valor(row.get('TARIFA'))
                peaje = limpiar_valor(row.get('PEAJE'))
                
                # Saltar filas vacías
                if not comercializadora and not tarifa:
                    continue
                
                # Validaciones básicas
                if not comercializadora:
                    result['errores_detalle'].append(f"Fila {fila_num}: COMERCIALIZADORA es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not tipo_energia:
                    result['errores_detalle'].append(f"Fila {fila_num}: ENERGIA es obligatorio")
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
                
                # Preparar valores para inserción
                # Nota: Potencias y Energías se guardan como strings en el modelo actual
                sql = """
                INSERT INTO tarifasenergia (
                    empresa, tipo, nombre, 
                    potencia1, potencia2, potencia3, potencia4, potencia5, potencia6,
                    energia1, energia2, energia3, energia4, energia5, energia6,
                    comision, precioNew,
                    tipo_cliente, peaje, termino_fijo_gas, pvd_sva, termino_variable_gas,
                    descuento, observaciones_descuentos, permanencia, excedentes,
                    bateria_virtual, fecha_carga,
                    termino_fijo_diario, precio_potencia_p1, precio_potencia_p2, precio_potencia_p3,
                    precio_energia_p1, precio_energia_p2, precio_energia_p3
                ) VALUES (
                    %s, %s, %s,
                    %s, %s, %s, %s, %s, %s,
                    %s, %s, %s, %s, %s, %s,
                    %s, %s,
                    %s, %s, %s, %s, %s,
                    %s, %s, %s, %s,
                    %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s
                )
                """
                
                valores = (
                    comercializadora, tipo_energia, tarifa,
                    str(potencia1) if potencia1 else None,
                    str(potencia2) if potencia2 else None,
                    str(potencia3) if potencia3 else None,
                    str(potencia4) if potencia4 else None,
                    str(potencia5) if potencia5 else None,
                    str(potencia6) if potencia6 else None,
                    str(energia1) if energia1 else None,
                    str(energia2) if energia2 else None,
                    str(energia3) if energia3 else None,
                    str(energia4) if energia4 else None,
                    str(energia5) if energia5 else None,
                    str(energia6) if energia6 else None,
                    comision if comision else 0,
                    precio_new,
                    tipo_cliente, peaje, termino_fijo_gas, pvd_sva, termino_variable_gas,
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
                
                if result['importados'] % 10 == 0:
                    print(f"  Procesadas {result['importados']} tarifas...")
                
            except Exception as e:
                result['errores'] += 1
                result['errores_detalle'].append(f"Fila {fila_num}: {str(e)}")
                print(f"[ERROR] Fila {fila_num}: {str(e)}")
        
        # Confirmar cambios
        conexion.commit()
        cursor.close()
        conexion.close()
        
        print(f"\n[OK] Importación completada")
        print(f"  Importados: {result['importados']}")
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
