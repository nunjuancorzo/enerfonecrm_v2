#!/usr/bin/env python3
"""
Script para importar tarifas y servicios desde archivos Excel a la base de datos MySQL
Soporta: Tarifas de Energía, Tarifas de Telefonía, Tarifas de Alarmas y Servicios

Uso: 
  python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx
  python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx
  python3 importar_tarifas_servicios.py tarifa-alarmas plantilla_tarifas_alarmas.xlsx
  python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx
"""

import sys
import pandas as pd
import mysql.connector
from datetime import datetime
from mysql.connector import Error

# Configuración de la base de datos
DB_CONFIG = {
    'host': 'localhost',
    'database': 'enerfone_pre',  # Cambiar según tu base de datos
    'user': 'root',  # Cambiar según tu usuario
    'password': ''  # Añadir tu contraseña
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
    return valor_str in ['sí', 'si', 'yes', 'true', '1', 's', 'y']

def importar_tarifas_energia(archivo_excel):
    """Importa tarifas de energía desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Tarifas Energía')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"✓ Se encontraron {len(df)} filas")
        
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
                
                # Saltar filas vacías
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
                
                # SQL de inserción
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
                print(f"✓ Fila {fila_num}: {nombre} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: Error - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"❌ {error_msg}")
        
        if result['importados'] > 0:
            conexion.commit()
        
        cursor.close()
        conexion.close()
        
    except Exception as e:
        result['errores_detalle'].append(f"Error general: {str(e)}")
    
    return result

def importar_tarifas_telefonia(archivo_excel):
    """Importa tarifas de telefonía desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Tarifas Telefonía')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"✓ Se encontraron {len(df)} filas")
        
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
                
                # Saltar filas vacías
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
                
                # SQL de inserción
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
                print(f"✓ Fila {fila_num}: {compania} - {tipo} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: Error - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"❌ {error_msg}")
        
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
        
        print(f"✓ Se encontraron {len(df)} filas")
        
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
                
                # Saltar filas vacías
                if not tipo and not nombre_tarifa:
                    continue
                
                # Validaciones
                if not tipo:
                    result['errores_detalle'].append(f"Fila {fila_num}: Tipo es obligatorio")
                    result['errores'] += 1
                    continue
                
                if tipo not in ['Kit', 'Opcional', 'Campaña']:
                    result['errores_detalle'].append(f"Fila {fila_num}: Tipo debe ser 'Kit', 'Opcional' o 'Campaña'")
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
                
                # SQL de inserción
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
                print(f"✓ Fila {fila_num}: {nombre_tarifa} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: Error - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"❌ {error_msg}")
        
        if result['importados'] > 0:
            conexion.commit()
        
        cursor.close()
        conexion.close()
        
    except Exception as e:
        result['errores_detalle'].append(f"Error general: {str(e)}")
    
    return result

def importar_servicios(archivo_excel):
    """Importa servicios desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Servicios')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"✓ Se encontraron {len(df)} filas")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        for index, row in df.iterrows():
            fila_num = index + 2
            
            try:
                # Campos obligatorios
                tipo = limpiar_valor(row.get('Tipo*'))
                nombre_servicio = limpiar_valor(row.get('NombreServicio*'))
                precio = limpiar_valor(row.get('Precio*'))
                
                # Saltar filas vacías
                if not tipo and not nombre_servicio:
                    continue
                
                # Validaciones
                if not tipo:
                    result['errores_detalle'].append(f"Fila {fila_num}: Tipo es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not nombre_servicio:
                    result['errores_detalle'].append(f"Fila {fila_num}: NombreServicio es obligatorio")
                    result['errores'] += 1
                    continue
                
                if not precio:
                    result['errores_detalle'].append(f"Fila {fila_num}: Precio es obligatorio")
                    result['errores'] += 1
                    continue
                
                # Preparar datos
                datos = {
                    'tipo': tipo,
                    'nombre_servicio': nombre_servicio,
                    'precio': precio,
                    'empresa': limpiar_valor(row.get('Empresa'))
                }
                
                # SQL de inserción
                sql = """
                    INSERT INTO servicios (tipo, nombreServicio, precio, empresa)
                    VALUES (%(tipo)s, %(nombre_servicio)s, %(precio)s, %(empresa)s)
                """
                
                cursor.execute(sql, datos)
                result['importados'] += 1
                print(f"✓ Fila {fila_num}: {nombre_servicio} - Importado")
                
            except Exception as e:
                result['errores'] += 1
                error_msg = f"Fila {fila_num}: Error - {str(e)}"
                result['errores_detalle'].append(error_msg)
                print(f"❌ {error_msg}")
        
        if result['importados'] > 0:
            conexion.commit()
        
        cursor.close()
        conexion.close()
        
    except Exception as e:
        result['errores_detalle'].append(f"Error general: {str(e)}")
    
    return result

def mostrar_resumen(resultado, tipo):
    """Muestra el resumen de la importación"""
    print(f"\n{'='*60}")
    print(f"✅ IMPORTACIÓN DE {tipo.upper()} COMPLETADA")
    print(f"{'='*60}")
    print(f"Registros importados: {resultado['importados']}")
    print(f"Filas con errores: {resultado['errores']}")
    
    if resultado['errores_detalle']:
        print(f"\n{'='*60}")
        print("ERRORES DETECTADOS:")
        print(f"{'='*60}")
        for error in resultado['errores_detalle']:
            print(f"❌ {error}")
    
    print(f"\n✓ Proceso finalizado")

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Uso: python3 importar_tarifas_servicios.py <tipo> <archivo_excel>")
        print("\nTipos disponibles:")
        print("  tarifa-energia    - Para tarifas de energía")
        print("  tarifa-telefonia  - Para tarifas de telefonía")
        print("  tarifa-alarmas    - Para tarifas de alarmas")
        print("  servicios         - Para servicios")
        print("\nEjemplos:")
        print("  python3 importar_tarifas_servicios.py tarifa-energia plantilla_tarifas_energia.xlsx")
        print("  python3 importar_tarifas_servicios.py tarifa-telefonia plantilla_tarifas_telefonia.xlsx")
        print("  python3 importar_tarifas_servicios.py tarifa-alarmas plantilla_tarifas_alarmas.xlsx")
        print("  python3 importar_tarifas_servicios.py servicios plantilla_servicios.xlsx")
        sys.exit(1)
    
    tipo = sys.argv[1].lower()
    archivo = sys.argv[2]
    
    print(f"""
{'='*60}
IMPORTACIÓN A LA BASE DE DATOS
{'='*60}
Tipo: {tipo.upper()}
Archivo: {archivo}
Base de datos: {DB_CONFIG['database']}
{'='*60}
    """)
    
    if tipo == 'tarifa-energia':
        resultado = importar_tarifas_energia(archivo)
        mostrar_resumen(resultado, 'Tarifas de Energía')
    elif tipo == 'tarifa-telefonia':
        resultado = importar_tarifas_telefonia(archivo)
        mostrar_resumen(resultado, 'Tarifas de Telefonía')
    elif tipo == 'tarifa-alarmas':
        resultado = importar_tarifas_alarmas(archivo)
        mostrar_resumen(resultado, 'Tarifas de Alarmas')
    elif tipo == 'servicios':
        resultado = importar_servicios(archivo)
        mostrar_resumen(resultado, 'Servicios')
    else:
        print(f"❌ Error: Tipo '{tipo}' no válido")
        print("Tipos válidos: tarifa-energia, tarifa-telefonia, tarifa-alarmas, servicios")
        sys.exit(1)
