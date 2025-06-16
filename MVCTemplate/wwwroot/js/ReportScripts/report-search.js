$(document).ready(function () {
    // Initially hide export filtered buttons
    $('#exportFilteredPdfBtn, #exportFilteredExcelBtn').hide();

    function toggleExportButtons() {
        const titleVal = $('#titleSearch').val().trim();
        const descVal = $('#descriptionSearch').val().trim();

        var table = $('#reportTable').DataTable();
        var filteredRowsCount = table.rows({ filter: 'applied' }).data().length;

        if ((titleVal.length > 0 || descVal.length > 0) && filteredRowsCount > 0) {
            $('#exportFilteredPdfBtn, #exportFilteredExcelBtn').show();
        } else {
            $('#exportFilteredPdfBtn, #exportFilteredExcelBtn').hide();
        }
    }

    // External inputs filter from js code columns
    $('#titleSearch').on('keyup change', function () {
        $('#reportTable').DataTable().column(0).search(this.value).draw();
        toggleExportButtons();
    });

    $('#descriptionSearch').on('keyup change', function () {
        $('#reportTable').DataTable().column(2).search(this.value).draw();
        toggleExportButtons();
    });

    // Export filtered PDF button event handler
    $('#exportFilteredPdfBtn').on('click', function () {
        var table = $('#reportTable').DataTable();
        var titleFilter = table.column(0).search() || '';
        var descriptionFilter = table.column(2).search() || '';

        var url = '/Admin/Report/ExportFilteredToPdf';
        var queryParams = [];

        if (titleFilter) {
            queryParams.push('titleFilter=' + encodeURIComponent(titleFilter));
        }
        if (descriptionFilter) {
            queryParams.push('descriptionFilter=' + encodeURIComponent(descriptionFilter));
        }

        if (queryParams.length > 0) {
            url += '?' + queryParams.join('&');
        }

        window.location.href = url; /* window.open(url, '_blank'); < opens a new tab*/
    });

    // Export filtered Excel button event handler
    $('#exportFilteredExcelBtn').on('click', function () {
        var table = $('#reportTable').DataTable();
        var titleFilter = table.column(0).search() || '';
        var descriptionFilter = table.column(2).search() || '';

        var url = '/Admin/Report/ExportFilteredToExcel';
        var queryParams = [];

        if (titleFilter) {
            queryParams.push('titleFilter=' + encodeURIComponent(titleFilter));
        }
        if (descriptionFilter) {
            queryParams.push('descriptionFilter=' + encodeURIComponent(descriptionFilter));
        }

        if (queryParams.length > 0) {
            url += '?' + queryParams.join('&');
        }

        window.location.href = url;  /*window.open(url, '_blank'); opens a new tab*/
    });
});
