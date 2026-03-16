#!/usr/bin/env python3
"""
Script para importar tarifas y servicios desde archivos Excel a la base de datos MySQL
Soporta: Tarifas de Energia, Tarifas de Telefonia, Tarifas de Alarmas y Servicios

Lee automaticamente la base de datos desde appsettings.Production.json

Uso: 
  python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx
  python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx
  python3 importar_tarifas_servicios.py tarifa-alarmas plantilla_tarifas_alarmas.xlsx
  python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx
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
    """Lee la configuracion de la base de datos desde appsettings.*.json"""
    try:
        # Buscar archivos de configuracion en orden de prioridad
        # PRODUCCION PRIMERO para que funcione correctamente en servidores
        config_files = [
            'appsettings.Production.json',               # Produccion generico (raiz) - PRIMERO
            'appsettings.Production.Enerfone.json',      # Produccion Enerfone (raiz)
            'appsettings.Production.GrupoBasette.json',  # Produccion GrupoBasette (raiz)
            'EnerfoneCRM/appsettings.Production.json',   # Produccion en subcarpeta
            'appsettings.json',                          # Generico (raiz) - Desarrollo
            'EnerfoneCRM/appsettings.json',              # Generico en subcarpeta (desarrollo)
            'appsettings.Development.json',              # Desarrollo (raiz)
            'EnerfoneCRM/appsettings.Development.json'   # Desarrollo en subcarpeta
        ]
        
        config_file = None
        for file_path in config_files:
            if os.path.exists(file_path):
                config_file = file_path
                print(f"[CONFIG] Usando archivo de configuracion: {config_file}")
                break
        
        if not config_file:
            print(f"[ERROR] Error: No se encuentra ningun archivo de configuracion")
            print(f"   Directorio actual: {os.getcwd()}")
            print(f"   Archivos buscados:")
            for cf in config_files:
                print(f"   - {cf}")
            sys.exit(1)
        
        with open(config_file, 'r', encoding='utf-8') as f:
            config = json.load(f)
        
        connection_string = config.get('ConnectionStrings', {}).get('DefaultConnection', '')
        if not connection_string:
            print(f"[ERROR] Error: No se encontro ConnectionStrings.DefaultConnection en {config_file}")
            sys.exit(1)
        
        # Extraer informacion de la cadena de conexion
        # Formato: Server=localhost;Port=3306;Database=nombre_bd;User=usuario;Password=contrasena;...
        db_match = re.search(r'Database=([^;]+)', connection_string)
        user_match = re.search(r'User=([^;]+)', connection_string)
        password_match = re.search(r'Password=([^;]+)', connection_string)
        host_match = re.search(r'Server=([^;]+)', connection_string)
        
        if not db_match:
            print(f"[ERROR] Error: No se pudo extraer el nombre de la base de datos")
            sys.exit(1)
        
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
        sys.exit(1)

# Verificar argumentos minimos
if len(sys.argv) < 3:
    print("Uso: python3 importar_tarifas_servicios.py <tipo> <archivo_excel>")
    print("\nTipos validos: tarifa-energia, tarifa-telefonia, tarifa-alarmas, servicios")
    print("\nEjemplos:")
    print("  python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx")
    print("  python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx")
    print("\nNOTA: El script lee automaticamente la base de datos desde appsettings.Production.json")
    sys.exit(1)

# Obtener configuracion automaticamente
DB_CONFIG = obtener_config_bd()

def limpiar_valor(valor):
    """Limpia valores None, NaN o vacios"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    return str(valor).strip()

def limpiar_decimal(valor):
    """Limpia y convierte valores decimales"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    try:
        return float(str(valor).replace(',', '.'))
    except:
        return None

def limpiar_entero(valor):
    """Limpia y convierte valores enteros"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    try:
        return int(float(str(valor)))
    except:
        return None

def limpiar_bool(valor):
    """Limpia y convierte valores booleanos"""
    if pd.isna(valor) or valor == '' or valor is None:
        return True  # Por defecto activo
    valor_str = str(valor).strip().lower()
    return valor_str in ['si', 'si', 'yes', 'true', '1', 's', 'y']

def importar_tarifas_energia(archivo_excel):
    """Importa tarifas de energia desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Tarifas Energia')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"[OK] Se encontraron {len(df)} filas")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        for index, row in df.iterrows():
            fila_num = index + 2
            
            try:
                # Campos obligatorios
                empresa = limpiar_valor(row.get('Empresa*'))
                tipo = limpiar_valor(row.get('Tipo*'))
                nombre = limpiar_valor(row.get('Nombre*'))
                potencia1 = limpiar_valor(row.get('Potencia1*'))
                energia1 = limpiar_valor(row.get('Energia1*'))
                comision = limpiar_decimal(row.get('Comision*'))
                precio_new = limpiar_decimal(row.get('PrecioNew*'))
                
                # Saltar filas vacias
                if not empresa and not nombre:
                    continue
                
                # Validaciones
                if not empresa:
                    result['errores_detalle'].append(f"Fila {fila_num}: Empresa es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not tipo:
                    result['errores_detalle'].append(f"Fila {fila_num}: Tipo es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not nombre:
                    result['errores_detalle'].append(f"Fila {fila_num}: Nombre es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not potencia1:
                    result['errores_detalle'].append(f"Fila {fila_num}: Potencia1 es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not energia1:
                    result['errores_detalle'].append(f"Fila {fila_num}: Energia1 es obligatorio")
                    result['errores'] += 1
                    continue
                
                if comision is None:
                    result['errores_detalle'].append(f"Fila {fila_num}: Comision es obligatorio")
                    result['errores'] += 1
                    continue
                
                if precio_new is None:
                    result['errores_detalle'].append(f"Fila {fila_num}: PrecioNew es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Preparar datos
                datos = {
                    'empresa': empresa,
                    'tipo': tipo,
                    'nombre': nombre,
                    'potencia1': potencia1,
                    'energia1': energia1,
                    'precio': limpiar_valor(row.get('Precio')),
                    'potencia2': limpiar_valor(row.get('Potencia2')),
                    'potencia3': limpiar_valor(row.get('Potencia3')),
                    'potencia4': limpiar_valor(row.get('Potencia4')),
                    'potencia5': limpiar_valor(row.get('Potencia5')),
                    'potencia6': limpiar_valor(row.get('Potencia6')),
                    'energia2': limpiar_valor(row.get('Energia2')),
                    'energia3': limpiar_valor(row.get('Energia3')),
                    'energia4': limpiar_valor(row.get('Energia4')),
                    'energia5': limpiar_valor(row.get('Energia5')),
                    'energia6': limpiar_valor(row.get('Energia6')),
                    'comision': comision,
                    'precio_new': precio_new,
                    'termino_fijo_diario': limpiar_decimal(row.get('TerminoFijoDiario')),
                    'precio_potencia_p1': limpiar_decimal(row.get('PrecioPotenciaP1')),
                    'precio_potencia_p2': limpiar_decimal(row.get('PrecioPotenciaP2')),
                    'precio_potencia_p3': limpiar_decimal(row.get('PrecioPotenciaP3')),
                    'precio_energia_p1': limpiar_decimal(row.get('PrecioEnergiaP1')),
                    'precio_energia_p2': limpiar_decimal(row.get('PrecioEnergiaP2')),
                    'precio_energia_p3': limpiar_decimal(row.get('PrecioEnergiaP3'))
                }
                
                # SQL de insercion
                sql = """
                    INSERT INTO tarifasenergia (
                        empresa, tipo, nombre, potencia1, energia1, precio,
                        potencia2, potencia3, potencia4, potencia5, potencia6,
                        energia2, energia3, energia4, energia5, energia6,
                        comision, precioNew, termino_fijo_diario,
                        precio_potencia_p1, precio_potencia_p2, precio_potencia_p3,
                        precio_energia_p1, precio_energia_p2, precio_energia_p3
                    ) VALUES (
                        %(empresa)s, %(tipo)s, %(nombre)s, %(potencia1)s, %(energia1)s, %(precio)s,
                        %(potencia2)s, %(potencia3)s, %(potencia4)s, %(potencia5)s, %(potencia6)s,
                        %(energia2)s, %(energia3)s, %(energia4)s, %(energia5)s, %(energia6)s,
                        %(comision)s, %(precio_new)s, %(termino_fijo_diario)s,
                        %(precio_potencia_p1)s, %(precio_potencia_p2)s, %(precio_potencia_p3)s,
                        %(precio_energia_p1)s, %(precio_energia_p2)s, %(precio_energia_p3)s
                    )
                """
                
                cursor.execute(sql, datos)
                result['importados'] += 1
                print(f"[OK] Fila {fila_num}: {nombre} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: Error - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"[ERROR] {error_msg}")
        
        if result['importados'] > 0:
            conexion.commit()
        
        cursor.close()
        conexion.close()
        
    except Exception as e:
        result['errores_detalle'].append(f"Error general: {str(e)}")
    
    return result

def importar_tarifas_telefonia(archivo_excel):
    """Importa tarifas de telefonia desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Tarifas Telefonia')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"[OK] Se encontraron {len(df)} filas")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        for index, row in df.iterrows():
            fila_num = index + 2
            
            try:
                # Campos obligatorios
                compania = limpiar_valor(row.get('Compania*'))
                tipo = limpiar_valor(row.get('Tipo*'))
                precio_new = limpiar_decimal(row.get('PrecioNew*'))
                comision_new = limpiar_decimal(row.get('ComisionNew*'))
                
                # Saltar filas vacias
                if not compania and not tipo:
                    continue
                
                # Validaciones
                if not compania:
                    result['errores_detalle'].append(f"Fila {fila_num}: Compania es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not tipo:
                    result['errores_detalle'].append(f"Fila {fila_num}: Tipo es obligatorio")
                    result['errores'] += 1
                    continue
                
                if precio_new is None:
                    result['errores_detalle'].append(f"Fila {fila_num}: PrecioNew es obligatorio")
                    result['errores'] += 1
                    continue
                
                if comision_new is None:
                    result['errores_detalle'].append(f"Fila {fila_num}: ComisionNew es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Preparar datos
                datos = {
                    'compania': compania,
                    'tipo': tipo,
                    'fibra': limpiar_valor(row.get('Fibra')),
                    'gb_movil': limpiar_valor(row.get('GbMovil')),
                    'precio': limpiar_valor(row.get('Precio')),
                    'comision': limpiar_valor(row.get('Comision')),
                    'precio_new': precio_new,
                    'comision_new': comision_new,
                    'tv': limpiar_valor(row.get('TV'))
                }
                
                # SQL de insercion
                sql = """
                    INSERT INTO tarifastelefonia (
                        compania, tipo, fibra, gbmovil, precio, comision,
                        precioNew, comisionNew, TV
                    ) VALUES (
                        %(compania)s, %(tipo)s, %(fibra)s, %(gb_movil)s, %(precio)s,
                        %(comision)s, %(precio_new)s, %(comision_new)s, %(tv)s
                    )
                """
                
                cursor.execute(sql, datos)
                result['importados'] += 1
                print(f"[OK] Fila {fila_num}: {compania} - {tipo} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: Error - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"[ERROR] {error_msg}")
        
        if result['importados'] > 0:
            conexion.commit()
        
        cursor.close()
        conexion.close()
        
    except Exception as e:
        result['errores_detalle'].append(f"Error general: {str(e)}")
    
    return result

def importar_tarifas_alarmas(archivo_excel):
    """Importa tarifas de alarmas desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Tarifas Alarmas')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"[OK] Se encontraron {len(df)} filas")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        for index, row in df.iterrows():
            fila_num = index + 2
            
            try:
                # Campos obligatorios
                tipo = limpiar_valor(row.get('Tipo*'))
                tipo_inmueble = limpiar_valor(row.get('TipoInmueble*'))
                nombre_tarifa = limpiar_valor(row.get('NombreTarifa*'))
                cuota_mensual = limpiar_decimal(row.get('CuotaMensual*'))
                permanencia = limpiar_entero(row.get('Permanencia*'))
                
                # Saltar filas vacias
                if not tipo and not nombre_tarifa:
                    continue
                
                # Validaciones
                if not tipo:
                    result['errores_detalle'].append(f"Fila {fila_num}: Tipo es obligatorio")
                    result['errores'] += 1
                    continue
                
                if tipo not in ['Kit', 'Opcional', 'Campana']:
                    result['errores_detalle'].append(f"Fila {fila_num}: Tipo debe ser 'Kit', 'Opcional' o 'Campana'")
                    result['errores'] += 1
                    continue
                
                if not tipo_inmueble:
                    result['errores_detalle'].append(f"Fila {fila_num}: TipoInmueble es obligatorio")
                    result['errores'] += 1
                    continue
                
                if tipo_inmueble not in ['Hogar', 'Negocio']:
                    result['errores_detalle'].append(f"Fila {fila_num}: TipoInmueble debe ser 'Hogar' o 'Negocio'")
                    result['errores'] += 1
                    continue
                
                if not nombre_tarifa:
                    result['errores_detalle'].append(f"Fila {fila_num}: NombreTarifa es obligatorio")
                    result['errores'] += 1
                    continue
                
                if cuota_mensual is None:
                    result['errores_detalle'].append(f"Fila {fila_num}: CuotaMensual es obligatorio")
                    result['errores'] += 1
                    continue
                
                if permanencia is None:
                    result['errores_detalle'].append(f"Fila {fila_num}: Permanencia es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Preparar datos
                datos = {
                    'tipo': tipo,
                    'tipo_inmueble': tipo_inmueble,
                    'nombre_tarifa': nombre_tarifa,
                    'cuota_mensual': cuota_mensual,
                    'permanencia': permanencia,
                    'empresa': limpiar_valor(row.get('Empresa')),
                    'comision': limpiar_decimal(row.get('Comision')),
                    'descripcion': limpiar_valor(row.get('Descripcion')),
                    'activa': limpiar_bool(row.get('Activa'))
                }
                
                # SQL de insercion
                sql = """
                    INSERT INTO tarifas_alarmas (
                        tipo, tipo_inmueble, nombre_tarifa, cuota_mensual, permanencia,
                        empresa, comision, descripcion, activa
                    ) VALUES (
                        %(tipo)s, %(tipo_inmueble)s, %(nombre_tarifa)s, %(cuota_mensual)s,
                        %(permanencia)s, %(empresa)s, %(comision)s, %(descripcion)s, %(activa)s
                    )
                """
                
                cursor.execute(sql, datos)
                result['importados'] += 1
                print(f"[OK] Fila {fila_num}: {nombre_tarifa} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: Error - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"[ERROR] {error_msg}")
        
        if result['importados'] > 0:
            conexion.commit()
        
        cursor.close()
        conexion.close()
        
    except Exception as e:
        result['errores_detalle'].append(f"Error general: {str(e)}")
    
    return result

def importar_servicios(archivo_excel):
    """Importa servicios desde un archivo Excel. Si tiene columna ID, actualiza; si no, inserta"""
    result = {'importados': 0, 'actualizados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Servicios')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        # Debug: mostrar columnas detectadas
        print(f"[DEBUG] Columnas detectadas en Excel: {list(df.columns)}")
        
        # Verificar si tiene columna ID
        tiene_columna_id = 'ID' in df.columns
        if tiene_columna_id:
            print(f"[INFO] Columna ID detectada - Se actualizaran servicios existentes")
        else:
            print(f"[INFO] Sin columna ID - Se insertaran servicios nuevos")
        
        print(f"[OK] Se encontraron {len(df)} filas")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        for index, row in df.iterrows():
            fila_num = index + 2
            
            try:
                # Leer ID si existe
                servicio_id = limpiar_entero(row.get('ID')) if tiene_columna_id else None
                
                # Campos obligatorios
                tipo = limpiar_valor(row.get('Tipo'))
                nombre_servicio = limpiar_valor(row.get('NombreServicio'))
                # Convertir precio (coma a punto para MySQL)
                precio_raw = limpiar_valor(row.get('Precio'))
                if precio_raw:
                    precio = str(precio_raw).replace(',', '.')
                else:
                    precio = None
                
                # Debug: mostrar valores leidos
                print(f"[DEBUG] Fila {fila_num}: ID={servicio_id}, Tipo={tipo}, Nombre={nombre_servicio}, Precio={precio}")
                
                # Saltar filas vacias
                if not tipo and not nombre_servicio:
                    print(f"[DEBUG] Fila {fila_num}: Saltada (vacia)")
                    continue
                
                # Validaciones
                if not tipo:
                    error_msg = f"Fila {fila_num}: Tipo es obligatorio"
                    result['errores_detalle'].append(error_msg)
                    result['errores'] += 1
                    print(f"[ERROR] {error_msg}")
                    continue
                
                if not nombre_servicio:
                    error_msg = f"Fila {fila_num}: NombreServicio es obligatorio"
                    result['errores_detalle'].append(error_msg)
                    result['errores'] += 1
                    print(f"[ERROR] {error_msg}")
                    continue
                
                if not precio:
                    error_msg = f"Fila {fila_num}: Precio es obligatorio"
                    result['errores_detalle'].append(error_msg)
                    result['errores'] += 1
                    print(f"[ERROR] {error_msg}")
                    continue
                
                # Preparar datos
                datos = {
                    'tipo': tipo,
                    'nombre_servicio': nombre_servicio,
                    'precio': precio,
                    'empresa': limpiar_valor(row.get('Empresa')) or limpiar_valor(row.get('EMPRESA'))
                }
                
                # Decidir si INSERT o UPDATE
                if tiene_columna_id and servicio_id:
                    # Verificar primero si el ID existe
                    cursor.execute("SELECT id FROM servicios WHERE id = %s", (servicio_id,))
                    existe = cursor.fetchone()
                    
                    if not existe:
                        error_msg = f"Fila {fila_num}: ID {servicio_id} no existe en la base de datos"
                        result['errores_detalle'].append(error_msg)
                        result['errores'] += 1
                        print(f"[ERROR] {error_msg}")
                        continue
                    
                    # Actualizar servicio existente
                    datos['id'] = servicio_id
                    sql = """
                        UPDATE servicios 
                        SET tipo = %(tipo)s, 
                            nombreServicio = %(nombre_servicio)s, 
                            precio = %(precio)s,
                            empresa = %(empresa)s
                        WHERE id = %(id)s
                    """
                    cursor.execute(sql, datos)
                    result['actualizados'] += 1
                    print(f"[OK] Fila {fila_num}: ID {servicio_id} - {nombre_servicio} - Actualizado")
                else:
                    # Insertar nuevo servicio
                    sql = """
                        INSERT INTO servicios (tipo, nombreServicio, precio, empresa)
                        VALUES (%(tipo)s, %(nombre_servicio)s, %(precio)s, %(empresa)s)
                    """
                    cursor.execute(sql, datos)
                    result['importados'] += 1
                    print(f"[OK] Fila {fila_num}: {nombre_servicio} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: {nombre_servicio if nombre_servicio else 'Sin nombre'} - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"[ERROR] {error_msg}")
        
        if result['importados'] > 0 or result['actualizados'] > 0:
            conexion.commit()
        
        cursor.close()
        conexion.close()
        
    except Exception as e:
        result['errores_detalle'].append(f"Error general: {str(e)}")
    
    return result

def mostrar_resumen(resultado, tipo):
    """Muestra el resumen de la importacion"""
    print(f"\n[OK] IMPORTACION DE {tipo.upper()} COMPLETADA")
    print(f"Importados: {resultado.get('importados', 0)}")
    if 'actualizados' in resultado:
        print(f"Actualizados: {resultado['actualizados']}")
    print(f"Errores: {resultado['errores']}")
    
    if resultado['errores_detalle']:
        print(f"\nERRORES DETECTADOS:")
        for error in resultado['errores_detalle']:
            # Usar [ERROR] para que sea reconocido por el parser de C#
            print(f"[ERROR] {error}")
    
    print(f"\n[OK] Proceso finalizado")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Uso: python3 importar_tarifas_servicios.py <tipo> <archivo_excel>")
        print("\nTipos disponibles:")
        print("  tarifa-energia    - Para tarifas de energia")
        print("  tarifa-telefonia  - Para tarifas de telefonia")
        print("  tarifa-alarmas    - Para tarifas de alarmas")
        print("  servicios         - Para servicios")
        print("\nEjemplos:")
        print("  python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx")
        print("  python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx")
        print("  python3 importar_tarifas_servicios.py tarifa-alarmas plantilla_tarifas_alarmas.xlsx")
        print("  python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx")
        print("\nNOTA: El script lee automaticamente la base de datos desde appsettings.Production.json")
        sys.exit(1)
    
    tipo = sys.argv[1].lower()
    archivo = sys.argv[2]
    
    print(f"[INFO] IMPORTACION A LA BASE DE DATOS")
    print(f"[INFO] Tipo: {tipo.upper()}")
    print(f"[INFO] Archivo: {archivo}")
    print(f"[INFO] Base de datos: {DB_CONFIG['database']} (detectada automaticamente)")
    
    if tipo == 'tarifa-energia':
        resultado = importar_tarifas_energia(archivo)
        mostrar_resumen(resultado, 'Tarifas de Energia')
    elif tipo == 'tarifa-telefonia':
        resultado = importar_tarifas_telefonia(archivo)
        mostrar_resumen(resultado, 'Tarifas de Telefonia')
    elif tipo == 'tarifa-alarmas':
        resultado = importar_tarifas_alarmas(archivo)
        mostrar_resumen(resultado, 'Tarifas de Alarmas')
    elif tipo == 'servicios':
        resultado = importar_servicios(archivo)
        mostrar_resumen(resultado, 'Servicios')
    else:
        print(f"[ERROR] Error: Tipo '{tipo}' no valido")
        print("Tipos validos: tarifa-energia, tarifa-telefonia, tarifa-alarmas, servicios")
        sys.exit(1)
