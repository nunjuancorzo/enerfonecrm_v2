#!/usr/bin/env python3
"""
Script para importar clientes desde un archivo Excel a la base de datos MySQL.

Lee automáticamente la base de datos desde uno de estos archivos:
- appsettings.Production.json
- appsettings.Production.Enerfone.json
- appsettings.Production.GrupoBasette.json

Uso:
    python3 importar_clientes.py <archivo_excel> [id_usuario]
"""

import sys
import os
import json
import re
import pandas as pd
import mysql.connector
from datetime import datetime
from mysql.connector import Error

try:
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    sys.stderr.reconfigure(encoding='utf-8', errors='replace')
except Exception:
    pass

def obtener_config_bd():
    """Lee la configuración de la base de datos desde appsettings de producción."""
    try:
        config_files = [
            'appsettings.Production.json',
            'appsettings.Production.Enerfone.json',
            'appsettings.Production.GrupoBasette.json'
        ]

        config_file = next((f for f in config_files if os.path.exists(f)), None)

        if not config_file:
            print("[ERROR] Error: No se encontró ningún archivo de configuración de producción")
            for f in config_files:
                print(f"   - {f}")
            print(f"   Directorio actual: {os.getcwd()}")
            print("   Ejecuta el script desde la carpeta raíz del proyecto")
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
    print("Uso: python3 importar_clientes.py <archivo_excel> [id_usuario]")
    print("\nNOTA: El script lee automáticamente la base de datos desde appsettings.Production.json")
    sys.exit(1)

archivo_excel_arg = sys.argv[1]

# Configuración de la base de datos (automática)
DB_CONFIG = obtener_config_bd()

def validar_email(email):
    """Valida formato básico de email"""
    if not email or pd.isna(email):
        return True
    import re
    pattern = r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$'
    return bool(re.match(pattern, str(email)))

def limpiar_valor(valor):
    """Limpia valores None, NaN o vacíos"""
    if pd.isna(valor) or valor == '' or valor is None:
        return None
    return str(valor).strip()

def importar_clientes(archivo_excel, id_usuario=1):
    """
    Importa clientes desde un archivo Excel a la base de datos
    
    Args:
        archivo_excel: Ruta al archivo Excel
        id_usuario: ID del usuario que realiza la importación (por defecto 1)
    """
    try:
        # Leer el archivo Excel
        print(f"Leyendo archivo: {archivo_excel}")
        df = pd.read_excel(archivo_excel, sheet_name='Clientes')
        
        # Normalizar nombres de columnas (quitar asteriscos)
        df.columns = df.columns.str.replace('*', '', regex=False).str.strip()
        
        # Verificar que tiene datos
        if df.empty:
            print("[ERROR] El archivo no contiene datos")
            return
        
        print(f"✓ Se encontraron {len(df)} filas")
        
        # Conectar a la base de datos
        print(f"Conectando a la base de datos {DB_CONFIG['database']}...")
        conexion = mysql.connector.connect(**DB_CONFIG)
        cursor = conexion.cursor()
        
        # Contador de éxitos y errores
        importados = 0
        errores = 0
        errores_detalle = []
        
        # Procesar cada fila
        for index, row in df.iterrows():
            fila_num = index + 2  # +2 porque Excel empieza en 1 y tiene encabezados
            
            try:
                # Verificar si tiene ID (para actualizar)
                id_cliente = row.get('ID')
                tiene_id = id_cliente is not None and not pd.isna(id_cliente)
                
                # Validar campos obligatorios
                tipo_cliente = limpiar_valor(row.get('TipoCliente'))
                nombre = limpiar_valor(row.get('Nombre'))
                
                # Saltar filas vacías
                if not tipo_cliente and not nombre:
                    continue
                
                if not tipo_cliente:
                    errores_detalle.append(f"Fila {fila_num}: TipoCliente es obligatorio")
                    errores += 1
                    continue
                
                # Mapear variantes de tipo de cliente
                tipo_cliente_map = {
                    'PYME': 'Pyme',
                    'EMPRESA': 'Pyme',
                    'PARTICULAR': 'Particular',
                    'AUTONOMO': 'Autónomo',
                    'AUTÓNOMO': 'Autónomo'
                }
                tipo_cliente_upper = tipo_cliente.upper()
                if tipo_cliente_upper in tipo_cliente_map:
                    tipo_cliente = tipo_cliente_map[tipo_cliente_upper]
                
                if tipo_cliente not in ['Particular', 'Pyme', 'Autónomo']:
                    errores_detalle.append(f"Fila {fila_num}: TipoCliente debe ser 'Particular', 'Autónomo' o 'Pyme' (actual: '{tipo_cliente}')")
                    errores += 1
                    continue
                
                if not nombre:
                    errores_detalle.append(f"Fila {fila_num}: Nombre es obligatorio")
                    errores += 1
                    continue
                
                # Validar email si existe
                email = limpiar_valor(row.get('Email'))
                if email and not validar_email(email):
                    errores_detalle.append(f"Fila {fila_num}: Email '{email}' no válido")
                    errores += 1
                    continue
                
                # Validar IBAN si existe
                iban = limpiar_valor(row.get('IBAN'))
                if iban and len(iban) > 34:
                    errores_detalle.append(f"Fila {fila_num}: IBAN no puede tener más de 34 caracteres")
                    errores += 1
                    continue
                
                # Preparar datos para insertar/actualizar
                dni_cif = limpiar_valor(row.get('DNI/CIF'))
                cnae = limpiar_valor(row.get('CNAE'))
                tipo_via = limpiar_valor(row.get('TipoVia'))
                procedencia = limpiar_valor(row.get('Procedencia'))
                
                datos = {
                    'tipo_cliente': tipo_cliente,
                    'nombre': nombre,
                    'dni_cif': dni_cif,
                    'cnae': cnae,
                    'dni_representante': limpiar_valor(row.get('DNI Representante')),
                    'email': email,
                    'telefono': limpiar_valor(row.get('Teléfono')),
                    'tipo_via': tipo_via,
                    'direccion': limpiar_valor(row.get('Dirección')),
                    'numero': limpiar_valor(row.get('Número')),
                    'escalera': limpiar_valor(row.get('Escalera')),
                    'piso': limpiar_valor(row.get('Piso')),
                    'puerta': limpiar_valor(row.get('Puerta')),
                    'aclarador': limpiar_valor(row.get('Aclarador')),
                    'poblacion': limpiar_valor(row.get('Población')),
                    'provincia': limpiar_valor(row.get('Provincia')),
                    'codigo_postal': limpiar_valor(row.get('Código Postal')),
                    'iban': iban,
                    'representante': limpiar_valor(row.get('Representante')),
                    'procedencia': procedencia,
                    'observaciones': limpiar_valor(row.get('Observaciones'))
                }
                
                # Validar y procesar ID_Usuario si existe
                id_usuario_valor = limpiar_valor(row.get('ID_Usuario'))
                id_usuario_validado = None
                nombre_usuario = None
                
                if id_usuario_valor:
                    try:
                        id_usuario_int = int(id_usuario_valor)
                        # Verificar que el usuario existe
                        cursor.execute("SELECT idusuarios, username, nombre, apellidos FROM usuarios WHERE idusuarios = %s", (id_usuario_int,))
                        usuario_resultado = cursor.fetchone()
                        cursor.fetchall()  # Consumir resultados restantes
                        
                        if usuario_resultado:
                            id_usuario_validado = id_usuario_int
                            # Construir nombre completo del usuario para el campo comercial
                            username = usuario_resultado[1] if usuario_resultado[1] else ''
                            nombre = usuario_resultado[2] if usuario_resultado[2] else ''
                            apellidos = usuario_resultado[3] if usuario_resultado[3] else ''
                            nombre_usuario = f"{nombre} {apellidos}".strip() if nombre or apellidos else username
                        else:
                            errores_detalle.append(f"Fila {fila_num}: ID_Usuario {id_usuario_int} no existe en la base de datos")
                            errores += 1
                            print(f"⚠ Fila {fila_num}: {nombre} - Usuario ID {id_usuario_int} no encontrado")
                            continue
                    except ValueError:
                        errores_detalle.append(f"Fila {fila_num}: ID_Usuario '{id_usuario_valor}' debe ser un número entero")
                        errores += 1
                        print(f"⚠ Fila {fila_num}: {nombre} - ID_Usuario no es válido")
                        continue
                
                # Agregar campos de usuario validados
                datos['id_usuario'] = id_usuario_validado
                datos['comercial'] = nombre_usuario
                
                if tiene_id:
                    # Actualizar cliente existente
                    id_cliente_int = int(id_cliente)
                    
                    # Verificar que el cliente existe
                    cursor.execute("SELECT id FROM clientes_simple WHERE id = %s", (id_cliente_int,))
                    resultado = cursor.fetchone()
                    cursor.fetchall()  # Consumir resultados restantes
                    
                    if not resultado:
                        errores_detalle.append(f"Fila {fila_num}: Cliente con ID {id_cliente_int} no existe en la base de datos")
                        errores += 1
                        print(f"⚠ Fila {fila_num}: Cliente ID {id_cliente_int} no encontrado")
                        continue
                    
                    # SQL de actualización
                    sql = """
                        UPDATE clientes_simple SET
                            tipo_cliente = %(tipo_cliente)s,
                            nombre = %(nombre)s,
                            dni_cif = %(dni_cif)s,
                            cnae = %(cnae)s,
                            dni_representante = %(dni_representante)s,
                            email = %(email)s,
                            telefono = %(telefono)s,
                            tipo_via = %(tipo_via)s,
                            direccion = %(direccion)s,
                            numero = %(numero)s,
                            escalera = %(escalera)s,
                            piso = %(piso)s,
                            puerta = %(puerta)s,
                            aclarador = %(aclarador)s,
                            poblacion = %(poblacion)s,
                            provincia = %(provincia)s,
                            codigo_postal = %(codigo_postal)s,
                            iban = %(iban)s,
                            representante = %(representante)s,
                            id_usuario = %(id_usuario)s,
                            comercial = %(comercial)s,
                            procedencia = %(procedencia)s,
                            observaciones = %(observaciones)s
                        WHERE id = %(id)s
                    """
                    
                    datos['id'] = id_cliente_int
                    cursor.execute(sql, datos)
                    importados += 1
                    print(f"✓ Fila {fila_num}: {nombre} (ID: {id_cliente_int}) - Actualizado correctamente")
                    
                else:
                    # Verificar si ya existe un cliente con ese DNI/CIF
                    cliente_existente = None
                    if dni_cif:
                        cursor.execute("SELECT id FROM clientes_simple WHERE dni_cif = %s", (dni_cif,))
                        resultado = cursor.fetchone()
                        cursor.fetchall()  # Consumir resultados restantes
                        if resultado:
                            cliente_existente = resultado[0]
                    
                    if cliente_existente:
                        # Cliente ya existe, saltarlo
                        errores_detalle.append(f"Fila {fila_num}: Cliente con DNI/CIF '{dni_cif}' ya existe (ID: {cliente_existente})")
                        errores += 1
                        print(f"⚠ Fila {fila_num}: {nombre} - Ya existe con DNI/CIF '{dni_cif}'")
                    else:
                        # Agregar campos de auditoría para INSERT
                        datos['fecha_alta'] = datetime.now()
                        
                        # SQL de inserción
                        # Nota: id_usuario es el campo principal, comercial es redundante pero se mantiene por compatibilidad
                        sql = """
                            INSERT INTO clientes_simple (
                                tipo_cliente, nombre, dni_cif, cnae, dni_representante, email, telefono,
                                tipo_via, direccion, numero, escalera, piso, puerta, aclarador,
                                poblacion, provincia, codigo_postal, iban, representante,
                                id_usuario, comercial, procedencia, observaciones, fecha_alta
                            ) VALUES (
                                %(tipo_cliente)s, %(nombre)s, %(dni_cif)s, %(cnae)s, %(dni_representante)s, 
                                %(email)s, %(telefono)s, %(tipo_via)s, %(direccion)s, %(numero)s, %(escalera)s, 
                                %(piso)s, %(puerta)s, %(aclarador)s, %(poblacion)s, %(provincia)s, 
                                %(codigo_postal)s, %(iban)s, %(representante)s, %(id_usuario)s, %(comercial)s, 
                                %(procedencia)s, %(observaciones)s, %(fecha_alta)s
                            )
                        """
                        
                        cursor.execute(sql, datos)
                        importados += 1
                        print(f"✓ Fila {fila_num}: {nombre} - Importado correctamente")
                
            except Exception as e:
                errores += 1
                error_msg = f"Fila {fila_num}: Error al procesar - {str(e)}"
                errores_detalle.append(error_msg)
                print(f"[ERROR] {error_msg}")
        
        # Confirmar cambios
        if importados > 0:
            conexion.commit()
        
        # Cerrar conexión
        cursor.close()
        conexion.close()
        
        # Imprimir resultado en formato parseable
        print(f"Importados: {importados}")
        print(f"Errores: {errores}")
        if errores_detalle:
            for error in errores_detalle:
                print(f"[ERROR] {error}")
        
    except FileNotFoundError:
        print(f"[ERROR] Error: No se encontró el archivo '{archivo_excel}'")
        print("Importados: 0")
        print("Errores: 1")
    except Exception as e:
        print(f"[ERROR] Error general: {str(e)}")
        print("Importados: 0")
        print("Errores: 1")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    # ID de usuario opcional (segundo argumento)
    id_usuario = int(sys.argv[2]) if len(sys.argv) > 2 else 1
    
    print(f"""
{'='*60}
IMPORTACIÓN DE CLIENTES A LA BASE DE DATOS
{'='*60}
Archivo: {archivo_excel_arg}
Base de datos: {DB_CONFIG['database']}
Usuario ID: {id_usuario}
{'='*60}
    """)
    
    importar_clientes(archivo_excel_arg, id_usuario)

