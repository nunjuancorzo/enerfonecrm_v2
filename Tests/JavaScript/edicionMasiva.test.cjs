const { test } = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const script = fs.readFileSync(path.join(__dirname, '../../EnerfoneCRM/wwwroot/js/edicionMasiva.js'), 'utf8');

function escenario(definiciones) {
    let pegar;
    const llamadas = [];
    const errores = [];
    const filas = definiciones.map(defs => ({
        cells: defs.map(def => ({
            input: {
                tagName: 'INPUT', type: 'text', value: '', step: '', maxLength: -1, eventos: [],
                ...def,
                matches() { return true; },
                dispatchEvent(evento) { this.eventos.push(evento.type); }
            },
            querySelector() { return this.input; }
        }))
    }));
    const cuerpo = { rows: filas };
    filas.forEach(fila => {
        fila.parentElement = cuerpo;
        fila.cells.forEach((celda, index) => {
            celda.cellIndex = index;
            celda.closest = () => fila;
            celda.input.closest = () => celda;
        });
    });
    const tabla = { contains: target => filas.some(f => f.cells.some(c => c.input === target)) };
    const window = { alert: error => errores.push(error) };
    vm.runInNewContext(script, {
        window, Event, Date,
        console: { error() {} },
        document: {
            addEventListener: (tipo, funcion) => { assert.equal(tipo, 'paste'); pegar = funcion; },
            getElementById: id => id === 'tabla' ? tabla : null
        }
    });
    const referencia = { invokeMethodAsync: async (...args) => llamadas.push(args) };
    window.initializeBulkPaste('tabla', referencia);
    return {
        filas, llamadas, errores, window,
        async pegar(texto, target = filas[0].cells[0].input) {
            let prevenido = false;
            await pegar({ target, clipboardData: { getData: () => texto }, preventDefault() { prevenido = true; } });
            return prevenido;
        }
    };
}

for (const texto of ['3.45', '3,45', ' 3.45 ', '+3,45']) {
    test(`celda decimal: ${texto}`, async () => {
        const contexto = escenario([[{ type: 'number', step: '0.01' }]]);
        await contexto.pegar(texto);
        assert.equal(contexto.filas[0].cells[0].input.value, '3.45');
        assert.deepEqual(contexto.filas[0].cells[0].input.eventos, ['input', 'change']);
        assert.deepEqual(contexto.errores, []);
    });
}

test('rango: conserva precisión, texto, ceros iniciales e IDs de solo lectura', async () => {
    const contexto = escenario([
        [{ readOnly: true, value: '41' }, { type: 'number', step: '0.00001' }, {}],
        [{ readOnly: true, value: '42' }, { type: 'number', step: '0.00001' }, {}]
    ]);
    await contexto.pegar('99\t0,12345\t001234\r\n100\t0.98765\tES0012', contexto.filas[0].cells[0].input);
    assert.equal(contexto.filas[0].cells[0].input.value, '41');
    // El rango empieza en la primera celda editable.
    await contexto.pegar('0,12345\t001234\r\n0.98765\tES0012', contexto.filas[0].cells[1].input);
    assert.equal(contexto.filas[0].cells[1].input.value, '0.12345');
    assert.equal(contexto.filas[1].cells[1].input.value, '0.98765');
    assert.equal(contexto.filas[0].cells[2].input.value, '001234');
    assert.equal(contexto.filas[1].cells[2].input.value, 'ES0012');
});

test('valor decimal inválido no modifica parcialmente un rango', async () => {
    const contexto = escenario([[{ type: 'number', step: '0.01', value: '5' }, { type: 'number', step: '0.01', value: '6' }]]);
    await contexto.pegar('3,45\t1.234,56');
    assert.equal(contexto.filas[0].cells[0].input.value, '5');
    assert.equal(contexto.filas[0].cells[1].input.value, '6');
    assert.equal(contexto.errores.length, 1);
});

test('no convierte identificadores enteros ni campos de texto', async () => {
    const contexto = escenario([[{ type: 'number', step: '1', value: '12' }, {}]]);
    await contexto.pegar('1.23');
    assert.equal(contexto.filas[0].cells[0].input.value, '12');
    assert.equal(contexto.errores.length, 1);
    await contexto.pegar('1,23', contexto.filas[0].cells[1].input);
    assert.equal(contexto.filas[0].cells[1].input.value, '1,23');
});

test('fechas y nuevos indicadores se distribuyen por celdas', async () => {
    const opciones = [{ value: '0', text: 'No' }, { value: '1', text: 'Sí' }];
    const contexto = escenario([[{ type: 'date' }, { tagName: 'SELECT', options: opciones }]]);
    await contexto.pegar('15/10/2026\tsí\n');
    assert.equal(contexto.filas[0].cells[0].input.value, '2026-10-15');
    assert.equal(contexto.filas[0].cells[1].input.value, '1');
    await contexto.pegar('31/02/2026');
    assert.equal(contexto.errores.length, 1);
    assert.equal(contexto.filas[0].cells[0].input.value, '2026-10-15');
});

test('pegado fuera de inputs invoca una sola vez el parser de filas de la tabla activa', async () => {
    const contexto = escenario([[{}]]);
    contexto.window.initializeBulkPaste('tabla', { invokeMethodAsync: async (...args) => contexto.llamadas.push(args) });
    await contexto.pegar('ID\tComision\n41\t3.45', { matches: () => false });
    assert.equal(contexto.llamadas.length, 1);
    assert.deepEqual(contexto.llamadas[0], ['PegarDesdePortapapeles', 'ID\tComision\n41\t3.45']);
});

test('no intercepta el pegado en inputs ajenos a la tabla', async () => {
    const contexto = escenario([[{}]]);
    const prevenido = await contexto.pegar('3,45', { matches: () => true });
    assert.equal(prevenido, false);
    assert.equal(contexto.llamadas.length, 0);
});
