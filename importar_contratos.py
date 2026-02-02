#!/usr/bin/env python3
"""
Script para importar contratos desde archivos Excel a la base de datos MySQL
Soporta tres tipos de contratos: Energía, Telefonía y Alarmas

Uso: 
  python3 importar_contratos.py energia plantilla_contratos_energia.xlsx
  python3 importar_contratos.py telefonia plantilla_contratos_telefonia.xlsx
  python3 importar_contratos.py alarmas plantilla_contratos_alarmas.xlsx
"""

import sys
import pandas as pd
import mysql.connector
from datetime import datetime
from mysql.connector import Error

# Configuración de la base de datos
DB_CONFIG = {
    'host': 'localhost',
    'database': 'enerfonecrm',  # Cambiar según tu base de datos
    'user': 'root',  # Cambiar según tu usuario
    'password': 'A76262136.r'  # Añadir tu contraseña
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

def limpiar_fecha(valor):
    """Limpia y convierte fechas"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    try:
        if isinstance(valor, datetime):
            return valor
        return pd.to_datetime(str(valor)).strftime('%Y-%m-%d')
    except:
        return None

def limpiar_bool(valor):
    """Limpia y convierte valores booleanos"""
    if pd.isna(valor) or valor == '' or valor is None:
        return False
    valor_str = str(valor).strip().lower()
    return valor_str in ['sí', 'si', 'yes', 'true', '1', 's', 'y']

def obtener_id_cliente_por_dni(cursor, dni_cif):
    """Obtiene el ID del cliente a partir de su DNI/CIF"""
    if not dni_cif:
        return None
    cursor.execute("SELECT id FROM clientes_simple WHERE dni_cif = %s", (dni_cif,))
    resultado = cursor.fetchone()
    return resultado[0] if resultado else None

def obtener_id_cliente_por_dni(cursor, dni_cif):
    """Obtiene el ID del cliente a partir de su DNI/CIF"""
    if not dni_cif:
        return None
    cursor.execute("SELECT id FROM clientes_simple WHERE dni_cif = %s", (dni_cif,))
    resultado = cursor.fetchone()
    return resultado[0] if resultado else None

def verificar_cliente_existe(cursor, id_cliente):
    """Verifica si un cliente existe en la base de datos"""
    cursor.execute("SELECT COUNT(*) FROM clientes_simple WHERE id = %s", (id_cliente,))
    return cursor.fetchone()[0] > 0

def importar_contratos_energia(archivo_excel):
    """Importa contratos de energía desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Contratos Energía')
        
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
                # Campo obligatorio - usar DNI/CIF en lugar de IdCliente
                dni_cif = limpiar_valor(row.get('DNI/CIF*'))
                
                # Saltar filas vacías
                if not dni_cif:
                    continue
                
                # Obtener el ID del cliente a partir del DNI/CIF
                id_cliente = obtener_id_cliente_por_dni(cursor, dni_cif)
                
                if not id_cliente:
                    result['errores_detalle'].append(f"Fila {fila_num}: No se encontró cliente con DNI/CIF '{dni_cif}'")
                    result['errores'] += 1
                    continue
                
                # Preparar datos
                datos = {
                    'tipo': 'Energía',
                    'id_cliente': id_cliente,
                    'estado': limpiar_valor(row.get('Estado')) or 'Pendiente',
                    'comercial': limpiar_valor(row.get('Comercial')),
                    'fecha_alta': limpiar_fecha(row.get('FechaAlta')),
                    'fecha_creacion': datetime.now(),
                    'estado_servicio': limpiar_valor(row.get('EstadoServicio')),
                    'en_comercializadora': limpiar_valor(row.get('Comercializadora')),
                    'en_tarifa': limpiar_valor(row.get('Tarifa')),
                    'en_cups': limpiar_valor(row.get('CUPS')),
                    'en_cups_gas': limpiar_valor(row.get('CUPSGas')),
                    'en_servicios': limpiar_valor(row.get('Servicios')),
                    'en_iban': limpiar_valor(row.get('IBAN')),
                    'tipo_operacion': limpiar_valor(row.get('TipoOperacion')),
                    'potencia_contratada': limpiar_decimal(row.get('PotenciaContratada')),
                    'consumo_anual': limpiar_decimal(row.get('ConsumoAnual')),
                    'comision': limpiar_decimal(row.get('Comision')),
                    'observaciones_estado': limpiar_valor(row.get('Observaciones'))
                }
                
                # SQL de inserción
                sql = """
                    INSERT INTO contratos (
                        tipo, idCliente, estado, comercial, fecha_alta, fecha_creacion,
                        estadoServicio, en_Comercializadora, en_Tarifa, en_CUPS, en_CUPSGas,
                        en_Servicios, en_IBAN, tipoOperacion, potencia_contratada_p1,
                        consumo_ultimos_12_meses, comision, observaciones_estado
                    ) VALUES (
                        %(tipo)s, %(id_cliente)s, %(estado)s, %(comercial)s, %(fecha_alta)s,
                        %(fecha_creacion)s, %(estado_servicio)s, %(en_comercializadora)s,
                        %(en_tarifa)s, %(en_cups)s, %(en_cups_gas)s, %(en_servicios)s,
                        %(en_iban)s, %(tipo_operacion)s, %(potencia_contratada)s,
                        %(consumo_anual)s, %(comision)s, %(observaciones_estado)s
                    )
                """
                
                cursor.execute(sql, datos)
                result['importados'] += 1
                print(f"✓ Fila {fila_num}: Contrato para cliente {id_cliente} - Importado")
                
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

def importar_contratos_telefonia(archivo_excel):
    """Importa contratos de telefonía desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Contratos Telefonía')
        
        if df.empty:
            result['errores_detalle'].append("El archivo no contiene datos")
            return result
        
        print(f"✓ Se encontraron {len(df)} filas")
        
        # Leer líneas adicionales si existen
        lineas_adicionales = {}
        try:
            df_lineas = pd.read_excel(archivo_excel, sheet_name='Líneas Adicionales')
            for _, linea in df_lineas.iterrows():
                id_cliente = limpiar_entero(linea.get('IdCliente*'))
                num_linea = limpiar_entero(linea.get('NumeroLinea'))
                if id_cliente and num_linea:
                    if id_cliente not in lineas_adicionales:
                        lineas_adicionales[id_cliente] = {}
                    lineas_adicionales[id_cliente][num_linea] = {
                        'telefono': limpiar_valor(linea.get('Telefono')),
                        'tarifa': limpiar_valor(linea.get('Tarifa')),
                        'tipo': limpiar_valor(linea.get('TipoLinea')),
                        'icc': limpiar_valor(linea.get('CodigoICC'))
                    }
            print(f"✓ Se encontraron líneas adicionales para {len(lineas_adicionales)} clientes")
        except:
            print("⚠ No se encontró hoja 'Líneas Adicionales'")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        for index, row in df.iterrows():
            fila_num = index + 2
            
            try:
                # Campo obligatorio
                id_cliente = limpiar_entero(row.get('IdCliente*'))
                
                # Saltar filas vacías
                if not id_cliente:
                    continue
                
                # Verificar que el cliente existe
                if not verificar_cliente_existe(cursor, id_cliente):
                    result['errores_detalle'].append(f"Fila {fila_num}: El cliente con ID {id_cliente} no existe")
                    result['errores'] += 1
                    continue
                
                # Preparar datos básicos
                datos = {
                    'tipo': 'Telefonía',
                    'id_cliente': id_cliente,
                    'estado': limpiar_valor(row.get('Estado')) or 'Pendiente',
                    'comercial': limpiar_valor(row.get('Comercial')),
                    'fecha_alta': limpiar_fecha(row.get('FechaAlta')),
                    'fecha_creacion': datetime.now(),
                    'operadora_tel': limpiar_valor(row.get('Operadora')),
                    'tarifa_tel': limpiar_valor(row.get('Tarifa')),
                    'tipo_tarifa_tel': limpiar_valor(row.get('TipoTarifa')),
                    'fijo_tel': limpiar_valor(row.get('FijoTel')),
                    'linea_movil_principal': limpiar_valor(row.get('LineaMovilPrincipal')),
                    'tipo_linea_movil_principal': limpiar_valor(row.get('TipoLineaMovilPrincipal')),
                    'codigo_icc_principal': limpiar_valor(row.get('CodigoICCPrincipal')),
                    'numero_lineas_tel': limpiar_entero(row.get('NumeroLineas')) or 0,
                    'iban': limpiar_valor(row.get('IBAN')),
                    'comision': limpiar_decimal(row.get('Comision')),
                    'observaciones_estado': limpiar_valor(row.get('Observaciones'))
                }
                
                # Agregar líneas adicionales si existen
                if id_cliente in lineas_adicionales:
                    for num, linea in lineas_adicionales[id_cliente].items():
                        if num <= 5:  # Máximo 5 líneas adicionales
                            datos[f'telefono_linea{num}_tel'] = linea['telefono']
                            datos[f'tarifa_linea{num}_tel'] = linea['tarifa']
                            datos[f'tipo_linea{num}_tel'] = linea['tipo']
                            datos[f'codigo_icc_linea{num}_tel'] = linea['icc']
                
                # Construir SQL dinámicamente
                campos = ['tipo', 'idCliente', 'estado', 'comercial', 'fecha_alta', 'fecha_creacion',
                         'operadora_tel', 'Tarifa_tel', 'TipoTarifa_tel', 'fijo_tel',
                         'LineaMovilPrincipal', 'tipo_linea_movil_principal', 'codigo_icc_principal',
                         'numero_lineas_tel', 'iban', 'comision', 'observaciones_estado']
                
                placeholders = ['%(tipo)s', '%(id_cliente)s', '%(estado)s', '%(comercial)s',
                              '%(fecha_alta)s', '%(fecha_creacion)s', '%(operadora_tel)s',
                              '%(tarifa_tel)s', '%(tipo_tarifa_tel)s', '%(fijo_tel)s',
                              '%(linea_movil_principal)s', '%(tipo_linea_movil_principal)s',
                              '%(codigo_icc_principal)s', '%(numero_lineas_tel)s', '%(iban)s',
                              '%(comision)s', '%(observaciones_estado)s']
                
                # Agregar líneas adicionales al SQL si existen
                for i in range(1, 6):
                    if f'telefono_linea{i}_tel' in datos:
                        campos.extend([f'telefono_linea{i}_tel', f'tarifa_linea{i}_tel',
                                     f'tipo_linea{i}_tel', f'codigo_icc_linea{i}_tel'])
                        placeholders.extend([f'%(telefono_linea{i}_tel)s', f'%(tarifa_linea{i}_tel)s',
                                           f'%(tipo_linea{i}_tel)s', f'%(codigo_icc_linea{i}_tel)s'])
                
                sql = f"""
                    INSERT INTO contratos ({', '.join(campos)})
                    VALUES ({', '.join(placeholders)})
                """
                
                cursor.execute(sql, datos)
                result['importados'] += 1
                print(f"✓ Fila {fila_num}: Contrato para cliente {id_cliente} - Importado")
                
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

def importar_contratos_alarmas(archivo_excel):
    """Importa contratos de alarmas desde un archivo Excel"""
    result = {'importados': 0, 'errores': 0, 'errores_detalle': []}
    
    try:
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Contratos Alarmas')
        
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
                # Campo obligatorio
                id_cliente = limpiar_entero(row.get('IdCliente*'))
                
                # Saltar filas vacías
                if not id_cliente:
                    continue
                
                # Verificar que el cliente existe
                if not verificar_cliente_existe(cursor, id_cliente):
                    result['errores_detalle'].append(f"Fila {fila_num}: El cliente con ID {id_cliente} no existe")
                    result['errores'] += 1
                    continue
                
                # Preparar datos
                datos = {
                    'tipo': 'Alarmas',
                    'id_cliente': id_cliente,
                    'estado': limpiar_valor(row.get('Estado')) or 'Pendiente',
                    'comercial': limpiar_valor(row.get('Comercial')),
                    'fecha_alta': limpiar_fecha(row.get('FechaAlta')),
                    'fecha_creacion': datetime.now(),
                    'tipo_alarma': limpiar_valor(row.get('TipoAlarma')),
                    'subtipo_inmueble': limpiar_valor(row.get('SubtipoInmueble')),
                    'empresa_alarma': limpiar_valor(row.get('EmpresaAlarma')),
                    'kit_alarma': limpiar_valor(row.get('KitAlarma')),
                    'opcionales_alarma': limpiar_valor(row.get('OpcionalesAlarma')),
                    'campana_alarma': limpiar_valor(row.get('CampanaAlarma')),
                    'tiene_contrato_anterior': limpiar_bool(row.get('TieneContratoAnterior')),
                    'compania_anterior': limpiar_valor(row.get('CompaniaAnterior')),
                    'direccion_instalacion_alarma': limpiar_valor(row.get('DireccionInstalacion')),
                    'numero_instalacion': limpiar_valor(row.get('NumeroInstalacion')),
                    'piso_instalacion': limpiar_valor(row.get('PisoInstalacion')),
                    'codigo_postal_instalacion': limpiar_valor(row.get('CodigoPostalInstalacion')),
                    'provincia_instalacion': limpiar_valor(row.get('ProvinciaInstalacion')),
                    'localidad_instalacion': limpiar_valor(row.get('LocalidadInstalacion')),
                    'iban': limpiar_valor(row.get('IBAN')),
                    'comision': limpiar_decimal(row.get('Comision')),
                    'observaciones_alarma': limpiar_valor(row.get('Observaciones'))
                }
                
                # SQL de inserción
                sql = """
                    INSERT INTO contratos (
                        tipo, idCliente, estado, comercial, fecha_alta, fecha_creacion,
                        tipo_alarma, subtipo_inmueble, empresa_alarma, kit_alarma,
                        opcionales_alarma, campana_alarma, tiene_contrato_anterior,
                        compania_anterior, direccion_instalacion_alarma, numero_instalacion,
                        piso_instalacion, codigo_postal_instalacion, provincia_instalacion,
                        localidad_instalacion, iban, comision, observaciones_alarma
                    ) VALUES (
                        %(tipo)s, %(id_cliente)s, %(estado)s, %(comercial)s, %(fecha_alta)s,
                        %(fecha_creacion)s, %(tipo_alarma)s, %(subtipo_inmueble)s,
                        %(empresa_alarma)s, %(kit_alarma)s, %(opcionales_alarma)s,
                        %(campana_alarma)s, %(tiene_contrato_anterior)s, %(compania_anterior)s,
                        %(direccion_instalacion_alarma)s, %(numero_instalacion)s,
                        %(piso_instalacion)s, %(codigo_postal_instalacion)s,
                        %(provincia_instalacion)s, %(localidad_instalacion)s, %(iban)s,
                        %(comision)s, %(observaciones_alarma)s
                    )
                """
                
                cursor.execute(sql, datos)
                result['importados'] += 1
                print(f"✓ Fila {fila_num}: Contrato para cliente {id_cliente} - Importado")
                
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
    print(f"✅ IMPORTACIÓN DE CONTRATOS DE {tipo.upper()} COMPLETADA")
    print(f"{'='*60}")
    print(f"Contratos importados: {resultado['importados']}")
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
        print("Uso: python3 importar_contratos.py <tipo> <archivo_excel>")
        print("\nTipos disponibles:")
        print("  energia    - Para contratos de energía")
        print("  telefonia  - Para contratos de telefonía")
        print("  alarmas    - Para contratos de alarmas")
        print("\nEjemplos:")
        print("  python3 importar_contratos.py energia plantilla_contratos_energia.xlsx")
        print("  python3 importar_contratos.py telefonia plantilla_contratos_telefonia.xlsx")
        print("  python3 importar_contratos.py alarmas plantilla_contratos_alarmas.xlsx")
        sys.exit(1)
    
    tipo = sys.argv[1].lower()
    archivo = sys.argv[2]
    
    print(f"""
{'='*60}
IMPORTACIÓN DE CONTRATOS A LA BASE DE DATOS
{'='*60}
Tipo: {tipo.upper()}
Archivo: {archivo}
Base de datos: {DB_CONFIG['database']}
{'='*60}
    """)
    
    if tipo == 'energia':
        resultado = importar_contratos_energia(archivo)
        mostrar_resumen(resultado, 'Energía')
    elif tipo == 'telefonia':
        resultado = importar_contratos_telefonia(archivo)
        mostrar_resumen(resultado, 'Telefonía')
    elif tipo == 'alarmas':
        resultado = importar_contratos_alarmas(archivo)
        mostrar_resumen(resultado, 'Alarmas')
    else:
        print(f"❌ Error: Tipo '{tipo}' no válido")
        print("Tipos válidos: energia, telefonia, alarmas")
        sys.exit(1)
