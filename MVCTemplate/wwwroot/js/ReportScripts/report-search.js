$(document).ready(function () {
    // Initially hide export filtered buttons
    $('#button-pdfFiltered-report, #button-excelFiltered-report').hide();

    function toggleExportButtons() {
        const titleVal = $('#titleSearch').val().trim();
        const descVal = $('#descriptionSearch').val().trim();

        var table = $('#reportTable').DataTable();
        var filteredRowsCount = table.rows({ filter: 'applied' }).data().length;

        if ((titleVal.length > 0 || descVal.length > 0) && filteredRowsCount > 0) {
            $('#button-pdfFiltered-report, #button-excelFiltered-report').show();
        } else {
            $('#button-pdfFiltered-report, #button-excelFiltered-report').hide();
        }
    }

    // Input field filters
    $('#titleSearch').on('keyup change', function () {
        $('#reportTable').DataTable().column(0).search(this.value).draw();
        toggleExportButtons();
    });

    $('#descriptionSearch').on('keyup change', function () {
        $('#reportTable').DataTable().column(2).search(this.value).draw();
        toggleExportButtons();
    });

    // Shared export logic for filtered buttons
    function exportFilteredReport(btnSelector, exportUrlBase) {
        const btn = $(btnSelector);
        if (btn.prop('disabled')) return;

        btn.prop('disabled', true);
        const originalHtml = btn.html();
        btn.html(`<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...`);

        var table = $('#reportTable').DataTable();
        var titleFilter = table.column(0).search() || '';
        var descriptionFilter = table.column(2).search() || '';

        var queryParams = [];
        if (titleFilter) queryParams.push('titleFilter=' + encodeURIComponent(titleFilter));
        if (descriptionFilter) queryParams.push('descriptionFilter=' + encodeURIComponent(descriptionFilter));

        // Step 1: Request download token
        fetch('/Admin/Report/GenerateDownloadToken', {
            method: 'POST'
        })
            .then(response => {
                if (!response.ok) throw new Error("Token request failed");
                return response.json();
            })
            .then(data => {
                if (!data.token) throw new Error("No token received");

                // Step 2: Append token and query params to URL
                queryParams.push('token=' + encodeURIComponent(data.token));
                const fullUrl = exportUrlBase + '?' + queryParams.join('&');

                // Step 3: Trigger download
                window.location.href = fullUrl;
            })
            .catch(error => {
                console.error('Export failed:', error);
                alert('Error generating download token or exporting report.');
            })
            .finally(() => {
                setTimeout(() => {
                    btn.prop('disabled', false);
                    btn.html(originalHtml);
                }, 1000);
            });
    }

    // Event bindings with token logic
    $('#button-pdfFiltered-report').on('click', function () {
        exportFilteredReport('#button-pdfFiltered-report', '/Admin/Report/ExportFilteredToPdf');
    });

    $('#button-excelFiltered-report').on('click', function () {
        exportFilteredReport('#button-excelFiltered-report', '/Admin/Report/ExportFilteredToExcel');
    });
});
