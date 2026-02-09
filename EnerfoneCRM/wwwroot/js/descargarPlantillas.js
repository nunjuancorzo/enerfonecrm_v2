// Funciones para generar y descargar plantillas Excel

window.descargarPlantillaClientes = function() {
    const rows = [
        ['TipoCliente*', 'Nombre*', 'DNI/CIF', 'DNI Representante', 'Email', 'Teléfono', 'Dirección', 'Número', 'Escalera', 'Piso', 'Puerta', 'Aclarador', 'Población', 'Provincia', 'Código Postal', 'IBAN', 'Representante', 'Comercial', 'Observaciones'],
        ['Particular', 'Ejemplo Cliente S.L.', 'B12345678', '12345678A', 'cliente@ejemplo.com', '912345678', 'Calle Mayor', '10', 'A', '3', 'B', 'Junto al parque', 'Madrid', 'Madrid', '28001', 'ES1234567890123456789012', 'Juan Pérez', 'María García', 'Cliente potencial']
    ];
    
    descargarExcel(rows, 'plantilla_clientes.xlsx', 'Clientes');
};

window.descargarPlantillaContratosEnergia = function() {
    const rows = [
        ['IdCliente*', 'TipoContrato*', 'FechaContrato*', 'Estado*', 'CIF Titular', 'Nombre Titular', 'Dirección', 'Población', 'Código Postal', 'Provincia', 'Tipo Vía', 'Número', 'Escalera', 'Piso', 'Puerta', 'CUPS', 'Tarifa ATR', 'Comercializadora', 'Potencia', 'Consumo Anual', 'P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'Precio Final', 'Fecha Activación', 'Fecha Inicio Liquidación', 'Fecha Fin Liquidación', 'Importe Liquidación', 'Observaciones'],
        ['B12345678', 'Energía', '2026-01-15', 'Activo', 'B12345678', 'Empresa Ejemplo S.L.', 'Calle Mayor', 'Madrid', '28001', 'Madrid', 'Calle', '10', 'A', '3', 'B', 'ES0031234567890123456789AB', '3.0TD', 'Iberdrola', '15', '25000', '0.15', '0.13', '0.11', '', '', '', '0.14', '2026-02-01', '2026-02-01', '2026-03-01', '1250.50', 'Primer contrato de energía']
    ];
    
    descargarExcel(rows, 'plantilla_contratos_energia.xlsx', 'Contratos Energia');
};

window.descargarPlantillaContratosTelefonia = function() {
    const rows = [
        ['IdCliente*', 'TipoContrato*', 'FechaContrato*', 'Estado*', 'Línea', 'ICCID', 'Tipo ICC', 'Tarifa', 'Operadora', 'Precio Final', 'Fecha Activación', 'Fecha Inicio Liquidación', 'Fecha Fin Liquidación', 'Importe Liquidación', 'Observaciones'],
        ['B12345678', 'Telefonía', '2026-01-15', 'Activo', '612345678', '8934071234567890123', 'Datos', 'Tarifa 20GB', 'Orange', '25.50', '2026-02-01', '2026-02-01', '2026-03-01', '25.50', 'Línea principal']
    ];
    
    descargarExcel(rows, 'plantilla_contratos_telefonia.xlsx', 'Contratos Telefonia');
};

window.descargarPlantillaContratosAlarmas = function() {
    const rows = [
        ['IdCliente*', 'TipoContrato*', 'FechaContrato*', 'Estado*', 'Dirección Instalación', 'Población', 'Código Postal', 'Provincia', 'Tipo Vía', 'Número', 'Escalera', 'Piso', 'Puerta', 'Empresa Alarma', 'Tarifa', 'Precio Final', 'Fecha Activación', 'Fecha Inicio Liquidación', 'Fecha Fin Liquidación', 'Importe Liquidación', 'Observaciones'],
        ['B12345678', 'Alarmas', '2026-01-15', 'Activo', 'Calle Mayor', 'Madrid', '28001', 'Madrid', 'Calle', '10', 'A', '3', 'B', 'Securitas Direct', 'Básica', '45.00', '2026-02-01', '2026-02-01', '2026-03-01', '45.00', 'Sistema de alarma básico']
    ];
    
    descargarExcel(rows, 'plantilla_contratos_alarmas.xlsx', 'Contratos Alarmas');
};

function descargarExcel(rows, nombreArchivo, nombreHoja) {
    // Usar SheetJS para crear el archivo Excel
    if (typeof XLSX === 'undefined') {
        // Si no está disponible SheetJS, usar CSV simple
        const csv = rows.map(row => row.join('\t')).join('\n');
        const BOM = '\uFEFF';
        const blob = new Blob([BOM + csv], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.setAttribute('href', url);
        link.setAttribute('download', nombreArchivo.replace('.xlsx', '.csv'));
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        return;
    }
    
    // Crear workbook con SheetJS
    const worksheet = XLSX.utils.aoa_to_sheet(rows);
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, nombreHoja);
    XLSX.writeFile(workbook, nombreArchivo);
}
