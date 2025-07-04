﻿/*document.querySelector("#button-excel-package").addEventListener("click", async function () {
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
});*/ // uses js (button commented out)

document.querySelector("#button-to-excel-package").addEventListener("click", function () {
    const btn = this;

    if (btn.disabled) return;

    btn.disabled = true;
    const originalHtml = btn.innerHTML;
    btn.innerHTML = `<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...`;

    // Step 1: Request token
    fetch('/Admin/Package/GenerateDownloadToken', {
        method: 'POST',
        credentials: 'same-origin' 
    })
        .then(response => response.json())
        .then(data => {
            if (data.token) {
                // Step 2: Redirect to download URL with token
                const url = "/Admin/Package/ExportToExcel?token=" + encodeURIComponent(data.token);
                window.location.href = url;
            } else {
                alert('Failed to get download token.');
            }
        })
        .catch(() => {
            alert('Error generating download token.');
        })
        .finally(() => {
            setTimeout(() => {
                btn.disabled = false;
                btn.innerHTML = originalHtml;
            }, 1000);
        });
});

// for the button using a cs


