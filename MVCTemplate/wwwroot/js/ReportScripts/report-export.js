document.addEventListener('DOMContentLoaded', function () {
    const excelBtn = document.getElementById('button-to-excel-report');
    const pdfBtn = document.getElementById('button-to-pdf-report');

    // Pass filtered export URLs as the 3rd argument
    if (excelBtn) handleExportWithToken(excelBtn, '/Admin/Report/ExportToExcel', '/Admin/Report/ExportFilteredToExcel');
    if (pdfBtn) handleExportWithToken(pdfBtn, '/Admin/Report/ExportToPdf', '/Admin/Report/ExportFilteredToPdf');
});

function handleExportWithToken(button, baseExportUrl, filteredExportUrl = null) {
    const originalHtml = button.innerHTML;

    button.addEventListener('click', function (event) {
        if (button.classList.contains('disabled')) {
            event.preventDefault();
            return;
        }

        button.classList.add('disabled');
        button.style.pointerEvents = 'none';
        button.innerHTML = `<i class="fa-solid fa-spinner fa-spin" style="color: #fff; margin-right: 6px;"></i> Exporting...`;

        // Fetch token before exporting
        fetch('/Admin/Report/GenerateDownloadToken', {
            method: 'POST',
            credentials: 'same-origin'  // important if your token endpoint requires auth cookies
        })
            .then(response => {
                if (!response.ok) throw new Error("Token request failed");
                return response.json();
            })
            .then(data => {
                if (!data.token) throw new Error("Failed to retrieve download token.");

                // Use DataTables API to check filters
                const table = $('#reportTable').DataTable();
                const titleFilter = table.column(0).search() || '';
                const descriptionFilter = table.column(2).search() || '';
                const hasFilters = titleFilter.length > 0 || descriptionFilter.length > 0;

                // Build query params
                const params = new URLSearchParams();
                params.append('token', data.token);

                let exportUrl = baseExportUrl;

                if (hasFilters && filteredExportUrl) {
                    exportUrl = filteredExportUrl;
                    if (titleFilter) params.append('titleFilter', titleFilter);
                    if (descriptionFilter) params.append('descriptionFilter', descriptionFilter);
                }

                // Redirect to the export URL with token and filters if any
                window.location.href = `${exportUrl}?${params.toString()}`;
            })
            .catch(error => {
                console.error('Error fetching token:', error);
                alert('An error occurred while generating the download token.');
            })
            .finally(() => {
                setTimeout(() => {
                    button.classList.remove('disabled');
                    button.style.pointerEvents = 'auto';
                    button.innerHTML = originalHtml;
                }, 1000);
            });
    });
}
