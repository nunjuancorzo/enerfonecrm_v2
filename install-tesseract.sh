#!/bin/bash

# Script de instalación de Tesseract OCR para CorCRM
# Soporte: Ubuntu/Debian, macOS, CentOS/RHEL

set -e

echo "========================================"
echo "  Instalador de Tesseract OCR"
echo "  Para CorCRM - Módulo de Comparativas"
echo "========================================"
echo ""

# Detectar sistema operativo
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    if [ -f /etc/debian_version ]; then
        OS="debian"
        echo "✓ Sistema detectado: Debian/Ubuntu"
    elif [ -f /etc/redhat-release ]; then
        OS="redhat"
        echo "✓ Sistema detectado: CentOS/RHEL"
    else
        OS="linux"
        echo "✓ Sistema detectado: Linux genérico"
    fi
elif [[ "$OSTYPE" == "darwin"* ]]; then
    OS="macos"
    echo "✓ Sistema detectado: macOS"
else
    echo "❌ Sistema operativo no soportado: $OSTYPE"
    exit 1
fi

echo ""

# Función para instalar en Debian/Ubuntu
install_debian() {
    echo "Instalando Tesseract OCR y dependencias..."
    sudo apt update
    sudo apt install -y tesseract-ocr tesseract-ocr-spa poppler-utils imagemagick
    echo "✓ Instalación completada"
}

# Función para instalar en CentOS/RHEL
install_redhat() {
    echo "Instalando Tesseract OCR y dependencias..."
    sudo yum install -y epel-release
    sudo yum install -y tesseract tesseract-langpack-spa poppler-utils ImageMagick
    echo "✓ Instalación completada"
}

# Función para instalar en macOS
install_macos() {
    # Verificar si Homebrew está instalado
    if ! command -v brew &> /dev/null; then
        echo "❌ Homebrew no está instalado"
        echo "Instala Homebrew desde: https://brew.sh"
        exit 1
    fi
    
    echo "Instalando Tesseract OCR y dependencias..."
    brew install tesseract tesseract-lang poppler imagemagick
    echo "✓ Instalación completada"
}

# Instalar según el sistema operativo
case $OS in
    debian)
        install_debian
        ;;
    redhat)
        install_redhat
        ;;
    macos)
        install_macos
        ;;
    *)
        echo "❌ No se pudo determinar el método de instalación"
        exit 1
        ;;
esac

echo ""
echo "========================================"
echo "  Verificando instalación..."
echo "========================================"
echo ""

# Verificar Tesseract
if command -v tesseract &> /dev/null; then
    TESSERACT_VERSION=$(tesseract --version | head -n 1)
    echo "✓ Tesseract: $TESSERACT_VERSION"
else
    echo "❌ Tesseract no se instaló correctamente"
    exit 1
fi

# Verificar idioma español
if tesseract --list-langs 2>/dev/null | grep -q "spa"; then
    echo "✓ Idioma español (spa) instalado"
else
    echo "⚠️  Advertencia: Idioma español no detectado"
    echo "   Instale con: sudo apt install tesseract-ocr-spa (Debian/Ubuntu)"
fi

# Verificar poppler (pdftoppm)
if command -v pdftoppm &> /dev/null; then
    echo "✓ Poppler (pdftoppm) instalado"
else
    echo "⚠️  Advertencia: pdftoppm no encontrado"
    echo "   Se necesita para procesar PDFs"
fi

# Verificar ImageMagick
if command -v convert &> /dev/null; then
    echo "✓ ImageMagick instalado"
else
    echo "⚠️  Advertencia: ImageMagick no encontrado"
    echo "   Útil para preprocesar imágenes"
fi

echo ""
echo "========================================"
echo "  Configuración en Base de Datos"
echo "========================================"
echo ""
echo "Para usar Tesseract en CorCRM, ejecuta:"
echo ""
echo "UPDATE configuracion_empresa SET"
echo "    ocr_proveedor = 'tesseract',"
echo "    ocr_proveedor_secundario = 'azure',"
echo "    ocr_fallback_automatico = TRUE;"
echo ""
echo "O usa Tesseract como secundario (gratis):"
echo ""
echo "UPDATE configuracion_empresa SET"
echo "    ocr_proveedor = 'azure',"
echo "    ocr_proveedor_secundario = 'tesseract',"
echo "    ocr_fallback_automatico = TRUE;"
echo ""
echo "========================================"
echo "  ✓ Instalación completada con éxito"
echo "========================================"
echo ""
echo "Prueba básica:"
echo "  tesseract --version"
echo "  tesseract --list-langs"
echo ""
