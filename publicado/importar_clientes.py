#!/usr/bin/env python3
"""
Script para importar clientes desde un archivo Excel a la base de datos MySQL
Uso: python3 importar_clientes.py plantilla_clientes.xlsx
"""

import sys
import pandas as pd
import mysql.connector
from datetime import datetime
from mysql.connector import Error

# Configuración de la base de datos
DB_CONFIG = {
    'host': 'localhost',
    'database': 'enerfone_pre',
    'user': 'enerfone',
    'password': 'Salaiet6680.'
}

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
        
        print(f"[OK] Se encontraron {len(df)} filas")
        
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
                    'PARTICULAR': 'Particular'
                }
                tipo_cliente_upper = tipo_cliente.upper()
                if tipo_cliente_upper in tipo_cliente_map:
                    tipo_cliente = tipo_cliente_map[tipo_cliente_upper]
                
                if tipo_cliente not in ['Particular', 'Pyme']:
                    errores_detalle.append(f"Fila {fila_num}: TipoCliente debe ser 'Particular' o 'Pyme' (actual: '{tipo_cliente}')")
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
                
                # Preparar datos para insertar
                dni_cif = limpiar_valor(row.get('DNI/CIF'))
                
                datos = {
                    'tipo_cliente': tipo_cliente,
                    'nombre': nombre,
                    'dni_cif': dni_cif,
                    'dni_representante': limpiar_valor(row.get('DNI Representante')),
                    'email': email,
                    'telefono': limpiar_valor(row.get('Teléfono')),
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
                    'comercial': limpiar_valor(row.get('Comercial')),
                    'observaciones': limpiar_valor(row.get('Observaciones')),
                    'fecha_alta': datetime.now(),
                    'id_usuario': id_usuario
                }
                
                # Verificar si ya existe un cliente con ese DNI/CIF
                cliente_existente = None
                if dni_cif:
                    cursor.execute("SELECT id FROM clientes_simple WHERE dni_cif = %s", (dni_cif,))
                    resultado = cursor.fetchone()
                    # Consumir cualquier resultado restante
                    cursor.fetchall()
                    if resultado:
                        cliente_existente = resultado[0]
                
                if cliente_existente:
                    # Cliente ya existe, saltarlo
                    errores_detalle.append(f"Fila {fila_num}: Cliente con DNI/CIF '{dni_cif}' ya existe (ID: {cliente_existente})")
                    errores += 1
                    print(f"[AVISO] Fila {fila_num}: {nombre} - Ya existe con DNI/CIF '{dni_cif}'")
                else:
                    # SQL de inserción
                    sql = """
                        INSERT INTO clientes_simple (
                            tipo_cliente, nombre, dni_cif, dni_representante, email, telefono,
                            direccion, numero, escalera, piso, puerta, aclarador,
                            poblacion, provincia, codigo_postal, iban, representante,
                            comercial, observaciones, fecha_alta, id_usuario
                        ) VALUES (
                            %(tipo_cliente)s, %(nombre)s, %(dni_cif)s, %(dni_representante)s, 
                            %(email)s, %(telefono)s, %(direccion)s, %(numero)s, %(escalera)s, 
                            %(piso)s, %(puerta)s, %(aclarador)s, %(poblacion)s, %(provincia)s, 
                            %(codigo_postal)s, %(iban)s, %(representante)s, %(comercial)s, 
                            %(observaciones)s, %(fecha_alta)s, %(id_usuario)s
                        )
                    """
                    
                    cursor.execute(sql, datos)
                    importados += 1
                    print(f"[OK] Fila {fila_num}: {nombre} - Importado correctamente")
                
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
        print(f"[ERROR] No se encontró el archivo '{archivo_excel}'")
        print("Importados: 0")
        print("Errores: 1")
    except Exception as e:
        print(f"[ERROR] Error general: {str(e)}")
        print("Importados: 0")
        print("Errores: 1")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Uso: python3 importar_clientes.py <archivo_excel>")
        print("Ejemplo: python3 importar_clientes.py plantilla_clientes.xlsx")
        sys.exit(1)
    
    archivo = sys.argv[1]
    
    # ID de usuario opcional (segundo argumento)
    id_usuario = int(sys.argv[2]) if len(sys.argv) > 2 else 1
    
    print(f"""
{'='*60}
IMPORTACIÓN DE CLIENTES A LA BASE DE DATOS
{'='*60}
Archivo: {archivo}
Base de datos: {DB_CONFIG['database']}
Usuario ID: {id_usuario}
{'='*60}
    """)
    
    importar_clientes(archivo, id_usuario)
