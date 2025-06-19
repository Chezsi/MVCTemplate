
/*$(document).ready(function () {
    $('#button-to-excel-person').on('click', function () {
        window.location.href = '/Admin/Person/ExportToExcel';
    });
});*/ // ^ uses controller no button disabling

$(document).ready(function () {
    $('#button-to-excel-person').on('click', function () {
        const btn = $(this);

        if (btn.prop('disabled')) return;

        btn.prop('disabled', true);
        const originalHtml = btn.html();
        btn.html('<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...');

        // Step 1: Generate token from /Admin/Person/GenerateDownloadToken
        $.post('/Admin/Person/GenerateDownloadToken')
            .done(function (response) {
                if (response.token) {
                    // Step 2: Trigger the download using token
                    window.location.href = '/Admin/Person/ExportToExcel?token=' + encodeURIComponent(response.token);

                    setTimeout(() => {
                        btn.prop('disabled', false);
                        btn.html(originalHtml);
                    }, 1000);
                } else {
                    alert('Failed to get download token.');
                    btn.prop('disabled', false);
                    btn.html(originalHtml);
                }
            })
            .fail(function () {
                alert('Error generating download token.');
                btn.prop('disabled', false);
                btn.html(originalHtml);
            });
    });
}); // uses controller (updated)

/*document.querySelector("#button-to-excel-person").addEventListener("click", async function () {
    var table = $('#Persons').DataTable();
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Person/GetAllPersons');
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.name.localeCompare(b.name)); // Update if model uses a different property

    // Format date as Month Day, Year (e.g., June 3, 2025)
    function formatDateWordMDY(dateValue) {
        if (!dateValue) return "";
        const d = new Date(dateValue);
        if (isNaN(d)) return dateValue;
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
    }

    // Format full datetime for "Generated at"
    function formatDateTimeWordMDY(dateValue) {
        if (!dateValue) return "";
        const d = new Date(dateValue);
        if (isNaN(d)) return dateValue;
        return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + ", " +
            d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
    }

    const now = new Date();
    const datetimeString = formatDateTimeWordMDY(now);

    // Construct the worksheet data
    const ws_data = [
        ["Person Data"],
        [`Generated at: ${datetimeString}`],
        ["ID", "Name", "Position", "Category ID"],
        ...dataToExport.map(row => [
            row.id,
            row.name,
            row.position,
            row.categoryId
        ])
    ];

    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet(ws_data);

    const lastRow = ws_data.length;
    ws['!autofilter'] = { ref: `A3:D${lastRow}` };

    // Merge title and datetime rows (A1:D1 and A2:D2)
    ws['!merges'] = [
        { s: { r: 0, c: 0 }, e: { r: 0, c: 3 } },  // A1:D1
        { s: { r: 1, c: 0 }, e: { r: 1, c: 3 } }   // A2:D2
    ];

    // Calculate max width dynamically for "Position" column
    const positionColumnIndex = 2;
    const padding = 2;
    let maxPositionLength = ws_data.reduce((max, row, index) => {
        if (index < 3) return max; // Skip headers and metadata rows
        const val = row[positionColumnIndex];
        if (!val) return max;
        return Math.max(max, val.toString().length);
    }, 0);

    // Set column widths
    ws['!cols'] = [
        { wch: 10 },                         // ID
        { wch: 20 },                         // Name
        { wch: maxPositionLength + padding }, // Position
        { wch: 15 }                          // Category ID
    ];

    XLSX.utils.book_append_sheet(wb, ws, "Persons");
    XLSX.writeFile(wb, "Persons.xlsx");
});*/ // ^ uses JS (button commented out)

document.querySelector("#exportCurrentContract").addEventListener("submit", function (e) {
    e.preventDefault();  // prevent form submission

    const btn = document.querySelector("#exportContractsBtn");
    if (btn.disabled) return;

    btn.disabled = true;
    const originalHtml = btn.innerHTML;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...';

    // Step 1: Request download token
    fetch('/Admin/Person/GenerateDownloadToken', {
        method: 'POST'
    })
        .then(response => response.json())
        .then(data => {
            if (data.token) {
                // Step 2: Redirect to download with token
                const formAction = e.target.action;
                const urlWithToken = formAction + "?token=" + encodeURIComponent(data.token);
                window.location.href = urlWithToken;
            } else {
                alert('Failed to get download token.');
            }
        })
        .catch(error => {
            console.error('Token error:', error);
            alert('Error generating download token.');
        })
        .finally(() => {
            setTimeout(() => {
                btn.disabled = false;
                btn.innerHTML = originalHtml;
            }, 1000);
        });
}); // uses controller (updated)

/*document.querySelector("#button-export-current-contracts").addEventListener("click", async function () {
    try {
        const response = await fetch('/Admin/Person/ExportPersonsWithCurrentContracts');
        const data = await response.json();

        if (!data || data.length === 0) {
            alert('No current contracts found.');
            return;
        }

        function formatDateWordMDY(dateValue) {
            if (!dateValue) return "";
            const d = new Date(dateValue);
            if (isNaN(d)) return dateValue;
            return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
        }

        function formatDateTimeWordMDY(dateValue) {
            if (!dateValue) return "";
            const d = new Date(dateValue);
            if (isNaN(d)) return dateValue;
            return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' }) + ", " +
                d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
        }

        const now = new Date();
        const generatedAt = formatDateTimeWordMDY(now);

        const ws_data = [
            ["Current Contracts Export"],
            [`Generated at: ${generatedAt}`],
            ["Person ID", "Name", "Position", "Category ID", "Contract ID", "Contract Name", "Description", "Validity"],
            ...data.map(row => [
                row.person_Id,
                row.person_Name,
                row.person_Position,
                row.person_CategoryId,
                row.contract_Id,
                row.contract_Name,
                row.contract_Description,
                formatDateWordMDY(row.contract_Validity)
            ])
        ];

        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.aoa_to_sheet(ws_data);

        // Merge title and generated rows
        ws['!merges'] = [
            { s: { r: 0, c: 0 }, e: { r: 0, c: 7 } },  // A1:H1
            { s: { r: 1, c: 0 }, e: { r: 1, c: 7 } }   // A2:H2
        ];

        // Autofilter
        ws['!autofilter'] = { ref: `A3:H${ws_data.length}` };

        // Column widths
        ws['!cols'] = [
            { wch: 12 }, { wch: 20 }, { wch: 18 }, { wch: 15 },
            { wch: 12 }, { wch: 20 }, { wch: 30 }, { wch: 22 }
        ];

        // Export
        XLSX.utils.book_append_sheet(wb, ws, "CurrentContracts");
        XLSX.writeFile(wb, "CurrentContracts.xlsx");

    } catch (err) {
        console.error("Export error:", err);
        alert("Failed to export current contracts.");
    }
});*/ // uses js (button commented out)


