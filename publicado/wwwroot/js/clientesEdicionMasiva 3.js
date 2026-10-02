// Excel-like paste functionality for bulk client editing (simplified version)
let dotnetReference = null;

window.initializeExcelLikePaste = function (dotnetRef) {
    console.log('Initializing Excel-like paste functionality');
    
    dotnetReference = dotnetRef;
    const table = document.getElementById('clientesTable');
    if (!table) {
        console.warn('Table not found');
        return;
    }

    // Add paste event listener to all inputs, selects, and textareas in the table
    const editableElements = table.querySelectorAll('input:not([type="checkbox"]), select, textarea');
    
    editableElements.forEach(element => {
        element.addEventListener('paste', handlePaste);
    });
    
    // Add global Ctrl+V listener for pasting new rows
    document.addEventListener('keydown', handleGlobalPaste);
    
    console.log('Excel-like functionality initialized');
};

async function handleGlobalPaste(e) {
    // Only handle Ctrl+V (or Cmd+V on Mac)
    if ((e.ctrlKey || e.metaKey) && e.key === 'v') {
        // Don't interfere if user is typing in an input field
        const activeElement = document.activeElement;
        if (activeElement && (activeElement.tagName === 'INPUT' || 
            activeElement.tagName === 'TEXTAREA' || 
            activeElement.tagName === 'SELECT')) {
            return;
        }
        
        // Prevent default paste behavior
        e.preventDefault();
        
        try {
            // Read clipboard content
            const clipboardText = await navigator.clipboard.readText();
            
            if (clipboardText && dotnetReference) {
                // Call Blazor method to paste rows
                await dotnetReference.invokeMethodAsync('PegarDesdePortapapeles', clipboardText);
                console.log('Pasted from clipboard via Ctrl+V');
            }
        } catch (err) {
            console.error('Error reading clipboard:', err);
        }
    }
}

function handlePaste(e) {
    // Get the pasted data
    const pastedData = e.clipboardData.getData('text');
    
    if (!pastedData) return;
    
    // Check if data contains multiple rows/columns (from Excel)
    const hasMultipleRows = pastedData.includes('\n');
    const hasMultipleCols = pastedData.includes('\t');
    
    if (!hasMultipleRows && !hasMultipleCols) {
        // Simple paste - let default behavior handle it
        return;
    }
    
    // Prevent default paste behavior for multi-cell paste
    e.preventDefault();
    
    // Get current cell position
    const currentCell = e.target.closest('td');
    if (!currentCell) return;
    
    const currentRow = currentCell.closest('tr');
    if (!currentRow) return;
    
    const currentRowIndex = currentRow.rowIndex - 1; // Subtract 1 for header row
    const currentCellIndex = currentCell.cellIndex;
    
    // Parse pasted data into rows and columns
    const rows = pastedData.split('\n').map(row => row.split('\t'));
    
    // Get all table rows (excluding header)
    const tableBody = currentRow.closest('tbody');
    const tableRows = Array.from(tableBody.querySelectorAll('tr'));
    
    // Iterate through pasted data
    rows.forEach((rowData, rowOffset) => {
        const targetRowIndex = currentRowIndex + rowOffset;
        
        // Skip if we've run out of rows in the table
        if (targetRowIndex >= tableRows.length) return;
        
        const targetRow = tableRows[targetRowIndex];
        const cells = targetRow.querySelectorAll('td');
        
        rowData.forEach((cellData, colOffset) => {
            const targetCellIndex = currentCellIndex + colOffset;
            
            // Skip if we've run out of columns
            if (targetCellIndex >= cells.length) return;
            
            const targetCell = cells[targetCellIndex];
            const input = targetCell.querySelector('input:not([type="checkbox"]), select, textarea');
            
            if (input) {
                // Clean the data
                let cleanData = cellData.trim();
                
                // Handle different input types
                if (input.tagName === 'SELECT') {
                    // For select elements, try to find matching option
                    const options = Array.from(input.options);
                    const matchingOption = options.find(opt => 
                        opt.value.toLowerCase() === cleanData.toLowerCase() ||
                        opt.text.toLowerCase() === cleanData.toLowerCase()
                    );
                    
                    if (matchingOption) {
                        input.value = matchingOption.value;
                    }
                } else {
                    // For input/textarea, set value directly
                    input.value = cleanData;
                }
                
                // Trigger change event to update Blazor binding
                const event = new Event('input', { bubbles: true });
                input.dispatchEvent(event);
                
                const changeEvent = new Event('change', { bubbles: true });
                input.dispatchEvent(changeEvent);
            }
        });
    });
    
    console.log(`Pasted ${rows.length} rows x ${rows[0].length} columns from Excel`);
}

console.log('Excel-like functionality loaded');
