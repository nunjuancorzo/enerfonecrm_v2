#!/usr/bin/env python3
import mysql.connector
import sys

try:
    conn = mysql.connector.connect(
        host='localhost',
        user='root',
        password='A76262136.r',
        database='enerfonecrm'
    )
    cursor = conn.cursor()
    
    # Ver cuántas tarifas hay por peaje
    cursor.execute("""
        SELECT peaje, COUNT(*) as total 
        FROM tarifasenergia 
        WHERE empresa IS NOT NULL 
        GROUP BY peaje 
        ORDER BY total DESC
    """)
    
    print('=== TARIFAS POR PEAJE LUZ ===')
    for row in cursor.fetchall():
        print(f'{row[0]} : {row[1]} tarifas')
    
    print()
    
    cursor.execute("""
        SELECT peaje_gas, COUNT(*) as total 
        FROM tarifasenergia 
        WHERE empresa IS NOT NULL 
        GROUP BY peaje_gas 
        ORDER BY total DESC
    """)
    
    print('=== TARIFAS POR PEAJE GAS ===')
    for row in cursor.fetchall():
        print(f'{row[0]} : {row[1]} tarifas')
    
    cursor.close()
    conn.close()
except Exception as e:
    print(f'Error: {e}', file=sys.stderr)
    sys.exit(1)
