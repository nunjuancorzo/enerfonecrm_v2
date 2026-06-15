#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script para generar SQL de códigos postales desde Geonames
Descarga datos de http://download.geonames.org/export/zip/ES.zip
y genera un archivo SQL con todos los códigos postales de España
"""

import urllib.request
import zipfile
import os
import sys
from datetime import datetime

def descargar_geonames():
    """Descarga el archivo ZIP de códigos postales de España desde Geonames"""
    url = "http://download.geonames.org/export/zip/ES.zip"
    zip_path = "ES.zip"
    
    print("📥 Descargando datos de Geonames...")
    print(f"   URL: {url}")
    
    try:
        urllib.request.urlretrieve(url, zip_path)
        print(f"✅ Descarga completada: {zip_path}")
        return zip_path
    except Exception as e:
        print(f"❌ Error descargando: {e}")
        sys.exit(1)

def extraer_zip(zip_path):
    """Extrae el archivo ZIP y devuelve el path del archivo .txt"""
    print("\n📦 Extrayendo archivo ZIP...")
    
    try:
        with zipfile.ZipFile(zip_path, 'r') as zip_ref:
            zip_ref.extractall('.')
        
        txt_file = "ES.txt"
        if os.path.exists(txt_file):
            print(f"✅ Archivo extraído: {txt_file}")
            return txt_file
        else:
            print(f"❌ No se encontró el archivo {txt_file}")
            sys.exit(1)
    except Exception as e:
        print(f"❌ Error extrayendo: {e}")
        sys.exit(1)

def procesar_geonames(txt_file):
    """
    Procesa el archivo ES.txt de Geonames y genera datos de códigos postales
    
    Formato del archivo ES.txt (separado por tabulaciones):
    0: country code      : ES
    1: postal code       : 28001
    2: place name        : Madrid
    3: admin name1       : Comunidad de Madrid (será nuestra provincia)
    4: admin code1       : MD
    5: admin name2       : Madrid
    6: admin code2       : M
    7: admin name3       : 
    8: admin code3       : 
    9: latitude
    10: longitude
    11: accuracy
    """
    print(f"\n📊 Procesando archivo {txt_file}...")
    
    codigos_postales = []
    codigos_unicos = set()
    lineas_procesadas = 0
    lineas_descartadas = 0
    
    try:
        with open(txt_file, 'r', encoding='utf-8') as f:
            for linea in f:
                lineas_procesadas += 1
                
                # Dividir por tabulaciones
                campos = linea.strip().split('\t')
                
                if len(campos) < 4:
                    lineas_descartadas += 1
                    continue
                
                codigo_postal = campos[1].strip()
                ciudad = campos[2].strip()
                provincia = campos[3].strip()
                
                # Validaciones
                if not codigo_postal or len(codigo_postal) != 5:
                    lineas_descartadas += 1
                    continue
                
                if not codigo_postal.isdigit():
                    lineas_descartadas += 1
                    continue
                
                if not ciudad or not provincia:
                    lineas_descartadas += 1
                    continue
                
                # Evitar duplicados (mismo código postal)
                if codigo_postal in codigos_unicos:
                    lineas_descartadas += 1
                    continue
                
                codigos_unicos.add(codigo_postal)
                
                # Limpiar caracteres especiales para SQL
                ciudad_sql = ciudad.replace("'", "''")
                provincia_sql = provincia.replace("'", "''")
                
                codigos_postales.append({
                    'codigo': codigo_postal,
                    'ciudad': ciudad_sql,
                    'provincia': provincia_sql
                })
        
        print(f"✅ Procesamiento completado:")
        print(f"   - Líneas procesadas: {lineas_procesadas}")
        print(f"   - Líneas descartadas: {lineas_descartadas}")
        print(f"   - Códigos postales únicos: {len(codigos_postales)}")
        
        return sorted(codigos_postales, key=lambda x: x['codigo'])
        
    except Exception as e:
        print(f"❌ Error procesando archivo: {e}")
        sys.exit(1)

def generar_sql(codigos_postales, output_file):
    """Genera el archivo SQL con todos los códigos postales"""
    print(f"\n📝 Generando archivo SQL: {output_file}...")
    
    try:
        with open(output_file, 'w', encoding='utf-8') as f:
            # Encabezado
            f.write("-- =====================================================\n")
            f.write("-- Script: Maestro Completo de Códigos Postales España\n")
            f.write("-- Descripción: Datos importados desde Geonames\n")
            f.write(f"-- Fecha generación: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"-- Total códigos postales: {len(codigos_postales)}\n")
            f.write("-- Fuente: http://download.geonames.org/export/zip/ES.zip\n")
            f.write("-- =====================================================\n\n")
            
            # Crear tabla
            f.write("-- Crear tabla de códigos postales\n")
            f.write("CREATE TABLE IF NOT EXISTS codigos_postales (\n")
            f.write("    id INT AUTO_INCREMENT PRIMARY KEY,\n")
            f.write("    codigo_postal VARCHAR(5) NOT NULL,\n")
            f.write("    ciudad VARCHAR(100) NOT NULL,\n")
            f.write("    provincia VARCHAR(50) NOT NULL,\n")
            f.write("    activo BOOLEAN DEFAULT TRUE,\n")
            f.write("    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,\n")
            f.write("    fecha_modificacion DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,\n")
            f.write("    \n")
            f.write("    UNIQUE KEY unique_codigo_postal (codigo_postal),\n")
            f.write("    INDEX idx_ciudad (ciudad),\n")
            f.write("    INDEX idx_provincia (provincia),\n")
            f.write("    INDEX idx_activo (activo)\n")
            f.write(") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;\n\n")
            
            # Insertar datos en lotes de 1000 para evitar queries muy largas
            f.write("-- Insertar códigos postales (importados desde Geonames)\n")
            
            batch_size = 1000
            total_batches = (len(codigos_postales) + batch_size - 1) // batch_size
            
            for batch_num in range(total_batches):
                start_idx = batch_num * batch_size
                end_idx = min((batch_num + 1) * batch_size, len(codigos_postales))
                batch = codigos_postales[start_idx:end_idx]
                
                f.write(f"\n-- Lote {batch_num + 1} de {total_batches} (CP {batch[0]['codigo']} - {batch[-1]['codigo']})\n")
                f.write("INSERT INTO codigos_postales (codigo_postal, ciudad, provincia) VALUES\n")
                
                for idx, cp in enumerate(batch):
                    coma = "," if idx < len(batch) - 1 else ""
                    f.write(f"('{cp['codigo']}', '{cp['ciudad']}', '{cp['provincia']}'){coma}\n")
                
                f.write("ON DUPLICATE KEY UPDATE\n")
                f.write("    ciudad = VALUES(ciudad),\n")
                f.write("    provincia = VALUES(provincia);\n")
            
            # Verificación
            f.write("\n-- =====================================================\n")
            f.write("-- Verificación\n")
            f.write("-- =====================================================\n")
            f.write("SELECT \n")
            f.write("    COUNT(*) as total_codigos_postales,\n")
            f.write("    COUNT(DISTINCT provincia) as total_provincias,\n")
            f.write("    COUNT(DISTINCT ciudad) as total_ciudades\n")
            f.write("FROM codigos_postales;\n\n")
            f.write("SELECT 'Tabla codigos_postales creada correctamente con datos de Geonames' AS resultado;\n")
        
        print(f"✅ Archivo SQL generado exitosamente: {output_file}")
        print(f"   Tamaño: {os.path.getsize(output_file) / (1024*1024):.2f} MB")
        
    except Exception as e:
        print(f"❌ Error generando SQL: {e}")
        sys.exit(1)

def limpiar_archivos_temporales():
    """Limpia archivos temporales descargados"""
    archivos_temp = ['ES.zip', 'ES.txt', 'readme.txt']
    
    print("\n🧹 Limpiando archivos temporales...")
    for archivo in archivos_temp:
        if os.path.exists(archivo):
            os.remove(archivo)
            print(f"   ✓ Eliminado: {archivo}")

def main():
    print("=" * 70)
    print("  GENERADOR DE SQL PARA CÓDIGOS POSTALES DE ESPAÑA")
    print("  Fuente: Geonames (http://www.geonames.org/)")
    print("=" * 70)
    
    output_file = "ADD_MAESTRO_CODIGOS_POSTALES_COMPLETO.sql"
    
    # Paso 1: Descargar datos de Geonames
    zip_path = descargar_geonames()
    
    # Paso 2: Extraer ZIP
    txt_file = extraer_zip(zip_path)
    
    # Paso 3: Procesar datos
    codigos_postales = procesar_geonames(txt_file)
    
    # Paso 4: Generar SQL
    generar_sql(codigos_postales, output_file)
    
    # Paso 5: Limpiar archivos temporales
    limpiar_archivos_temporales()
    
    print("\n" + "=" * 70)
    print("✅ PROCESO COMPLETADO EXITOSAMENTE")
    print("=" * 70)
    print(f"\n📄 Archivo generado: {output_file}")
    print(f"📊 Total códigos postales: {len(codigos_postales)}")
    print("\n💡 Siguiente paso:")
    print(f"   mysql -u usuario -p nombre_db < {output_file}")
    print("=" * 70 + "\n")

if __name__ == "__main__":
    main()
