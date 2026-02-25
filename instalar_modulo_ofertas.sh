#!/bin/bash

# Script para instalar el módulo de Solicitudes de Ofertas
# Ejecuta este script para crear la tabla en la base de datos

echo "======================================"
echo "Instalación Módulo de Ofertas"
echo "======================================"
echo ""

# Verificar si existe el archivo SQL
if [ ! -f "ADD_SOLICITUDES_OFERTAS.sql" ]; then
    echo "❌ Error: No se encuentra el archivo ADD_SOLICITUDES_OFERTAS.sql"
    exit 1
fi

# Leer credenciales de base de datos
echo "Introduce los datos de conexión a MySQL:"
read -p "Host [localhost]: " DB_HOST
DB_HOST=${DB_HOST:-localhost}

read -p "Puerto [3306]: " DB_PORT
DB_PORT=${DB_PORT:-3306}

read -p "Usuario [enerfone]: " DB_USER
DB_USER=${DB_USER:-enerfone}

read -sp "Contraseña: " DB_PASSWORD
echo ""

read -p "Base de datos [enerfone_pre]: " DB_NAME
DB_NAME=${DB_NAME:-enerfone_pre}

echo ""
echo "Ejecutando script SQL..."

# Intentar ejecutar con mysql
if command -v mysql &> /dev/null; then
    mysql -h "$DB_HOST" -P "$DB_PORT" -u "$DB_USER" -p"$DB_PASSWORD" "$DB_NAME" < ADD_SOLICITUDES_OFERTAS.sql
    
    if [ $? -eq 0 ]; then
        echo "✅ Tabla 'solicitudes_ofertas' creada exitosamente"
        echo ""
        echo "El módulo de ofertas está listo para usar."
        echo "Accede a: Gestiones > Ofertas"
    else
        echo "❌ Error al ejecutar el script SQL"
        echo ""
        echo "Puedes ejecutar manualmente el archivo ADD_SOLICITUDES_OFERTAS.sql usando:"
        echo "- phpMyAdmin"
        echo "- MySQL Workbench"
        echo "- Otro cliente MySQL de tu preferencia"
    fi
else
    echo "⚠️  El comando 'mysql' no está disponible en tu sistema"
    echo ""
    echo "Por favor, ejecuta el archivo ADD_SOLICITUDES_OFERTAS.sql manualmente usando:"
    echo "- phpMyAdmin"
    echo "- MySQL Workbench"
    echo "- Otro cliente MySQL de tu preferencia"
    echo ""
    echo "Credenciales:"
    echo "Host: $DB_HOST"
    echo "Puerto: $DB_PORT"
    echo "Usuario: $DB_USER"
    echo "Base de datos: $DB_NAME"
fi

echo ""
echo "======================================"
