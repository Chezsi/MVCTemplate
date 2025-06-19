document.querySelector("#button-to-excel-contract").addEventListener("click", async function () {
    const btn = this;
    if (btn.disabled) return; // prevent multiple clicks

    btn.disabled = true;
    const originalText = btn.innerHTML;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...';

    const startTime = Date.now();

    try {
        var table = $('#Contracts').DataTable();
        var searchValue = table.search();
        var dataToExport;

        if (searchValue) {
            // Get filtered rows if a search value exists
            dataToExport = table.rows({ search: 'applied' }).data().toArray();
        } else {
            // Fetch all contracts from the server with the required header
            let response = await fetch('/Admin/Contract/GetAllContracts', {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                alert('Failed to fetch contract data: ' + response.statusText);
                return;
            }

            let result = await response.json();
            dataToExport = result.data;
        }

        dataToExport.sort((a, b) => a.name.localeCompare(b.name));

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

    } catch (error) {
        console.error("Error exporting to Excel:", error);
        alert("Failed to export contract data.");
    } finally {
        // Ensure button disabled for at least 1 second
        const elapsed = Date.now() - startTime;
        const remaining = 1000 - elapsed;
        setTimeout(() => {
            btn.disabled = false;
            btn.innerHTML = originalText;
        }, remaining > 0 ? remaining : 0);
    }
});
