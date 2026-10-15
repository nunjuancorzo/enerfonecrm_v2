(() => {
    let registro = null;

    window.initializeBulkPaste = (tableId, dotnetRef) => {
        registro = { tableId, dotnetRef };
    };

    function normalizar(input, texto) {
        let valor = texto.trim();
        if (input.type === 'number' && input.step && Number(input.step) < 1) {
            valor = valor.replace(',', '.');
            if (valor && !/^[+-]?(?:\d+(?:\.\d*)?|\.\d+)$/.test(valor)) {
                throw new Error('Número decimal no válido. Usa coma o punto sin separadores de miles.');
            }
            if (valor.startsWith('+')) valor = valor.slice(1);
            if (valor.endsWith('.')) valor = valor.slice(0, -1);
        } else if (input.type === 'number' && valor && !/^\d+$/.test(valor)) {
            throw new Error('Este campo requiere un número entero.');
        } else if (input.type === 'date' && valor) {
            const fecha = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/.exec(valor);
            if (fecha) valor = `${fecha[3]}-${fecha[2].padStart(2, '0')}-${fecha[1].padStart(2, '0')}`;
            const comprobacion = new Date(`${valor}T00:00:00Z`);
            if (!/^\d{4}-\d{2}-\d{2}$/.test(valor) || Number.isNaN(comprobacion.getTime())
                || comprobacion.toISOString().slice(0, 10) !== valor) {
                throw new Error('Fecha no válida. Usa dd/MM/yyyy o yyyy-MM-dd.');
            }
        }

        if (input.tagName === 'SELECT') {
            const booleanos = { true: '1', false: '0', si: '1', 'sí': '1', no: '0' };
            const opciones = Array.from(input.options);
            const esBooleano = opciones.some(o => o.value === '1' && o.text === 'Sí')
                && opciones.some(o => o.value === '0' && o.text === 'No');
            if (esBooleano) valor = booleanos[valor.toLowerCase()] ?? valor;
            const opcion = opciones.find(o => o.value.toLowerCase() === valor.toLowerCase()
                || o.text.toLowerCase() === valor.toLowerCase());
            if (!opcion) throw new Error(`Valor no disponible en el selector: ${valor}`);
            valor = opcion.value;
        }
        if (input.maxLength > 0 && valor.length > input.maxLength) {
            throw new Error(`Máximo ${input.maxLength} caracteres en este campo.`);
        }
        return valor;
    }

    function asignar(input, valor) {
        input.value = valor;
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }

    document.addEventListener('paste', async e => {
        if (!registro) return;
        const table = document.getElementById(registro.tableId);
        if (!table) return;
        const target = e.target;
        const editable = target.matches?.('input, textarea, select') ?? false;
        if (editable && !table.contains(target)) return;
        const texto = e.clipboardData?.getData('text/plain');
        if (!texto) return;

        e.preventDefault();
        try {
            if (!editable) {
                await registro.dotnetRef.invokeMethodAsync('PegarDesdePortapapeles', texto);
                return;
            }
            if (target.readOnly || target.disabled || target.type === 'checkbox') return;
            const filas = texto.replace(/\r\n?/g, '\n').replace(/\n+$/, '').split('\n').map(f => f.split('\t'));
            if (filas.length === 1 && filas[0].length === 1) {
                asignar(target, normalizar(target, filas[0][0]));
                return;
            }
            const celda = target.closest('td');
            const fila = celda?.closest('tr');
            if (!fila) return;
            const filasTabla = Array.from(fila.parentElement.rows);
            const inicio = filasTabla.indexOf(fila);
            const cambios = [];
            filas.forEach((columnas, offsetFila) => {
                const destino = filasTabla[inicio + offsetFila];
                if (!destino) return;
                columnas.forEach((valor, offsetColumna) => {
                    const input = destino.cells[celda.cellIndex + offsetColumna]
                        ?.querySelector('input:not([type="checkbox"]), select, textarea');
                    if (input && !input.readOnly && !input.disabled) {
                        cambios.push([input, normalizar(input, valor)]);
                    }
                });
            });
            cambios.forEach(([input, valor]) => asignar(input, valor));
        } catch (error) {
            console.error('Error al pegar en edición masiva:', error);
            window.alert(`No se pudo pegar la información: ${error.message}`);
        }
    });
})();
