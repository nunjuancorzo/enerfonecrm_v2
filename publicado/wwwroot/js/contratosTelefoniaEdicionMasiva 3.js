// Excel-like paste functionality for bulk telefonia contracts editing
let dotnetReference = null;

window.initializeContratosTelefoniaPaste = function (dotnetRef) {
    console.log('Initializing Contratos Telefonía paste functionality');
    
    dotnetReference = dotnetRef;
    const table = document.getElementById('contratosTelefoniaTable');
    if (!table) {
        console.warn('Table not found');
        return;
    }

    const editableElements = table.querySelectorAll('input:not([type="checkbox"]), select, textarea');
    
    editableElements.forEach(element => {
        element.addEventListener('paste', handlePaste);
    });
    
    document.addEventListener('keydown', handleGlobalPaste);
    
    console.log('Contratos Telefonía paste functionality initialized');
};

async function handleGlobalPaste(e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'v') {
        const activeElement = document.activeElement;
        if (activeElement && (activeElement.tagName === 'INPUT' || 
            activeElement.tagName === 'TEXTAREA' || 
            activeElement.tagName === 'SELECT')) {
            return;
        }
        
        e.preventDefault();
        
        try {
            const clipboardText = await navigator.clipboard.readText();
            
            if (clipboardText && dotnetReference) {
                await dotnetReference.invokeMethodAsync('PegarDesdePortapapeles', clipboardText);
                console.log('Pasted from clipboard via Ctrl+V');
            }
        } catch (err) {
            console.error('Error reading clipboard:', err);
        }
    }
}

function handlePaste(e) {
    const pastedData = e.clipboardData.getData('text');
    
    if (!pastedData) return;
    
    const hasMultipleRows = pastedData.includes('\n');
    const hasMultipleCols = pastedData.includes('\t');
    
    if (!hasMultipleRows && !hasMultipleCols) {
        return;
    }
    
    e.preventDefault();
    
    const currentCell = e.target.closest('td');
    if (!currentCell) return;
    
    const currentRow = currentCell.closest('tr');
    if (!currentRow) return;
    
    const currentRowIndex = currentRow.rowIndex - 1;
    const currentCellIndex = currentCell.cellIndex;
    
    const rows = pastedData.split('\n').map(row => row.split('\t'));
    
    const tableBody = currentRow.closest('tbody');
    const tableRows = Array.from(tableBody.querySelectorAll('tr'));
    
    rows.forEach((rowData, rowOffset) => {
        const targetRowIndex = currentRowIndex + rowOffset;
        
        if (targetRowIndex >= tableRows.length) return;
        
        const targetRow = tableRows[targetRowIndex];
        const cells = targetRow.querySelectorAll('td');
        
        rowData.forEach((cellData, colOffset) => {
            const targetCellIndex = currentCellIndex + colOffset;
            
            if (targetCellIndex >= cells.length) return;
            
            const targetCell = cells[targetCellIndex];
            const input = targetCell.querySelector('input:not([type="checkbox"]), select, textarea');
            
            if (input) {
                let cleanData = cellData.trim();
                
                if (input.tagName === 'SELECT') {
                    const options = Array.from(input.options);
                    const matchingOption = options.find(opt => 
                        opt.value.toLowerCase() === cleanData.toLowerCase() ||
                        opt.text.toLowerCase() === cleanData.toLowerCase()
                    );
                    
                    if (matchingOption) {
                        input.value = matchingOption.value;
                    }
                } else {
                    input.value = cleanData;
                }
                
                const event = new Event('input', { bubbles: true });
                input.dispatchEvent(event);
                
                const changeEvent = new Event('change', { bubbles: true });
                input.dispatchEvent(changeEvent);
            }
        });
    });
    
    console.log(`Pasted ${rows.length} rows x ${rows[0].length} columns`);
}

console.log('Contratos Telefonía paste functionality loaded');
