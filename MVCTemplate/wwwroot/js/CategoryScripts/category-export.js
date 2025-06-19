
/*$(document).ready(function () {
    $('#button-to-excel-category').on('click', function () {
        window.location.href = '/Admin/Category/ExportToExcel';
    });
});*/ // ^ uses controller no button disabling

$(document).ready(function () {
    $('#button-to-excel-category').on('click', function () {
        const btn = $(this);

        if (btn.prop('disabled')) {
            return; // prevent multiple clicks while disabled
        }

        btn.prop('disabled', true);
        const originalText = btn.text();
        btn.text('Exporting...');

        // Trigger download by changing location
        window.location.href = '/Admin/Category/ExportToExcel';

        // Re-enable button after 1 second (hardcoded)
        setTimeout(() => {
            btn.prop('disabled', false);
            btn.text(originalText);
        }, 1000);
    });
}); // ^ uses controller (updated)


document.querySelector("#button-to-excel-category").addEventListener("click", async function () {
    var table = $('#Categorys').DataTable();
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Category/GetAllCategory');
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.nameCategory.localeCompare(b.nameCategory));

    // Helper functions to format date and datetime
    function formatDateTimeWordMDY(dateValue) {
        if (!dateValue) return "";
        const d = new Date(dateValue);
        if (isNaN(d)) return dateValue;
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + ", " +
            d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
    }

    const now = new Date();
    const datetimeString = formatDateTimeWordMDY(now);

    // Build worksheet data
    const ws_data = [
        ["Category Data"],
        [`Generated at: ${datetimeString}`],
        ["ID", "Name", "Code"],
        ...dataToExport.map(row => [
            row.idCategory,
            row.nameCategory,
            row.codeCategory
        ])
    ];

    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(ws_data);

    const lastRow = ws_data.length;
    ws['!autofilter'] = { ref: `A3:C${lastRow}` };

    // Merge top title and generated-at rows
    ws['!merges'] = [
        { s: { r: 0, c: 0 }, e: { r: 0, c: 2 } },  // A1:C1
        { s: { r: 1, c: 0 }, e: { r: 1, c: 2 } }   // A2:C2
    ];

    // Dynamic width for "Name" and "Code"
    const padding = 2;

    const getMaxLength = (index) => {
        return ws_data.reduce((max, row, i) => {
            if (i < 3) return max; // skip title, date, and headers
            const val = row[index];
            return Math.max(max, val ? val.toString().length : 0);
        }, 0);
    };

    ws['!cols'] = [
        { wch: 10 },                            // ID
        { wch: getMaxLength(1) + padding },     // Name
        { wch: getMaxLength(2) + padding }      // Code
    ];

    XLSX.utils.book_append_sheet(wb, ws, "Categories");
    XLSX.writeFile(wb, "Category.xlsx");
}); // uses JS (button commented out)
