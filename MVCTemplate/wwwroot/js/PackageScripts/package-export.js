document.querySelector("#button-excel").addEventListener("click", async function () {
    var table = $('#Packages').DataTable();
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Package/GetAllPackages');
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.name.localeCompare(b.name));

    // Format date with full datetime
    function formatDateTimeWordMDY(dateValue) {
        if (!dateValue) return "";
        const d = new Date(dateValue);
        if (isNaN(d)) return dateValue;
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + ", " +
            d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
    }

    const now = new Date();
    const datetimeString = formatDateTimeWordMDY(now);

    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Packages');

    // Add title row
    worksheet.addRow(['Package Data']);
    worksheet.mergeCells('A1:D1');

    // Add generated-at row
    worksheet.addRow([`Generated at: ${datetimeString}`]);
    worksheet.mergeCells('A2:D2');

    // Add header row
    worksheet.addRow(['ID', 'Name', 'Description', 'Priority']);

    // Add data rows
    dataToExport.forEach(row => {
        worksheet.addRow([
            row.id,
            row.name,
            row.description,
            row.priority
        ]);
    });

    // Add autofilter to the actual data header row (which is the 3rd row now)
    worksheet.autoFilter = {
        from: 'A3',
        to: 'D3'
    };

    // Optionally adjust column widths dynamically
    worksheet.columns = [
        { header: 'ID', key: 'id', width: 10 },
        { header: 'Name', key: 'name', width: Math.max(...dataToExport.map(r => r.name?.length || 0), 4) + 4 },
        { header: 'Description', key: 'description', width: Math.max(...dataToExport.map(r => r.description?.length || 0), 11) + 4 },
        { header: 'Priority', key: 'priority', width: 12 }
    ];

    // Generate file buffer and download
    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = 'Package.xlsx';
    a.click();

    URL.revokeObjectURL(url);
});
