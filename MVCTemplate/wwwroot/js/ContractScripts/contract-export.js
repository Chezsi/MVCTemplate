document.querySelector("#button-to-excel-contract").addEventListener("click", async function () {
    var table = $('#Contracts').DataTable();
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Contract/GetAllContracts');
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.name.localeCompare(b.name));

    // Format date as Month Day, Year (e.g., June 3, 2025)
    function formatDateWordMDY(dateValue) {
        if (!dateValue) return "";
        const d = new Date(dateValue);
        if (isNaN(d)) return dateValue;
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    }

    // Format date with time for "Generated at" (e.g. June 3, 2025, 02:15 PM)
    function formatDateTimeWordMDY(dateValue) {
        if (!dateValue) return "";
        const d = new Date(dateValue);
        if (isNaN(d)) return dateValue;
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + ", " +
            d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
    }

    const now = new Date();
    const datetimeString = formatDateTimeWordMDY(now);

    const ws_data = [
        ["Contract Data"],
        [`Generated at: ${datetimeString}`],
        ["ID", "Name", "Description", "Validity", "Person ID"],
        ...dataToExport.map(row => [
            row.id,
            row.name,
            row.description,
            formatDateWordMDY(row.validity),
            row.personId
        ])
    ];

    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(ws_data);

    const lastRow = ws_data.length;
    ws['!autofilter'] = { ref: `A3:E${lastRow}` };

    ws['!merges'] = ws['!merges'] || [];
    ws['!merges'].push({ s: { r: 0, c: 0 }, e: { r: 0, c: 4 } });  // Merge A1:E1 (title)
    ws['!merges'].push({ s: { r: 1, c: 0 }, e: { r: 1, c: 4 } });  // Merge A2:E2 (generated at)

    // No styling applied (removed centering and bold)

    // Calculate max length in the Validity column (index 3)
    const validityColumnIndex = 3;
    let maxLength = ws_data.reduce((max, row) => {
        const cellValue = row[validityColumnIndex];
        if (!cellValue) return max;
        return Math.max(max, cellValue.toString().length);
    }, 0);

    const padding = 2;

    ws['!cols'] = [
        { wch: 10 },                    // ID
        { wch: 20 },                    // Name
        { wch: 30 },                    // Description
        { wch: maxLength + padding },  // Validity (dynamic width)
        { wch: 15 }                    // Person ID
    ];

    XLSX.utils.book_append_sheet(wb, ws, "Contracts");
    XLSX.writeFile(wb, "Contracts.xlsx");
});

// harder to style excel in js than cs