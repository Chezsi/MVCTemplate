
// Custom date range filter for DataTables
$.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
    let min = $('#startDate').val();
    let max = $('#endDate').val();
    let dateStr = data[2]; // "validity" column index

    if (!dateStr) return false;

    let createdDate = new Date(dateStr);
    createdDate.setHours(0, 0, 0, 0);

    let minDate = min ? new Date(min) : null;
    let maxDate = max ? new Date(max) : null;

    if (minDate) minDate.setHours(0, 0, 0, 0);
    if (maxDate) maxDate.setHours(23, 59, 59, 999);

    return (!minDate || createdDate >= minDate) &&
        (!maxDate || createdDate <= maxDate);
});

// Setup event handlers for filters
function setupContractSearchFilters(dataTable) {
    $('#nameSearch').on('keyup change', function () {
        dataTable.column(0).search(this.value).draw();
    });

    $('#descriptionSearch').on('keyup change', function () {
        dataTable.column(1).search(this.value).draw();
    });

    $('#startDate, #endDate').on('change', function () {
        dataTable.draw();
    });

    $('#personIdSearch').on('change', function () {
        dataTable.column(3).search(this.value).draw();
    });
}
