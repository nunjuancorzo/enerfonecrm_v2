// tarifasTelefoniaEdicionMasiva.js
// Proporciona funcionalidad estilo Excel para TarifasTelefoniaEdicionMasiva

let dotnetRefTarifasTelefonia = null;

window.initializeExcelLikePaste = function (dotnetRef) {
    dotnetRefTarifasTelefonia = dotnetRef;

    // Global Ctrl+V handler
    const handleGlobalPaste = async (e) => {
        const target = e.target;
        if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.tagName === 'SELECT') {
            return;
        }

        const table = document.getElementById('tarifasTelefoniaTable');
        if (!table) return;

        e.preventDefault();
        const clipboardData = e.clipboardData || window.clipboardData;
        const pastedText = clipboardData.getData('Text');

        if (!pastedText) return;

        if (dotnetRefTarifasTelefonia) {
            await dotnetRefTarifasTelefonia.invokeMethodAsync('PegarDesdePortapapeles', pastedText);
        }
    };

    document.addEventListener('paste', handleGlobalPaste);

    // Cell-level paste handler para multi-celdas
    const handlePaste = async (e) => {
        const target = e.target;
        if (target.tagName !== 'INPUT' && target.tagName !== 'TEXTAREA') {
            return;
        }

        const table = document.getElementById('tarifasTelefoniaTable');
        if (!table || !table.contains(target)) {
            return;
        }

        e.preventDefault();
        const clipboardData = e.clipboardData || window.clipboardData;
        const pastedText = clipboardData.getData('Text');

        if (!pastedText) return;

        const rows = pastedText.split('\n').filter(r => r.trim() !== '');
        if (rows.length <= 1 && rows[0].split('\t').length <= 1) {
            target.value = pastedText.trim();
            target.dispatchEvent(new Event('input', { bubbles: true }));
            target.dispatchEvent(new Event('change', { bubbles: true }));
            return;
        }

        const rowsData = rows.map(row => row.split('\t'));

        if (dotnetRefTarifasTelefonia) {
            await dotnetRefTarifasTelefonia.invokeMethodAsync('PasteRowsFromJS', rowsData);
        }
    };

    document.addEventListener('paste', handlePaste, true);
};
