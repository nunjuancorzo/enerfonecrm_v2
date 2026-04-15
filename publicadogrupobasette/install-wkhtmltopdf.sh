#!/bin/bash

# Script para instalar librerías nativas de wkhtmltopdf para DinkToPdf

echo "Instalando librerías nativas de wkhtmltopdf..."

# Crear directorio
mkdir -p wkhtmltox/"64 bit"

cd wkhtmltox/"64 bit"

# Detectar sistema operativo y descargar librería apropiada
if [[ "$OSTYPE" == "darwin"* ]]; then
    echo "Descargando librería para macOS..."
    # Para macOS, descarga la librería dylib
    curl -L -o libwkhtmltox.dylib https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.dylib
    chmod +x libwkhtmltox.dylib
    echo "Librería para macOS instalada correctamente"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    echo "Descargando librería para Linux..."
    curl -L -o libwkhtmltox.so https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.so
    chmod +x libwkhtmltox.so
    echo "Librería para Linux instalada correctamente"
else
    echo "Sistema operativo no soportado. Por favor, descarga manualmente la librería apropiada."
fi

cd ../..

echo "Instalación completa"
