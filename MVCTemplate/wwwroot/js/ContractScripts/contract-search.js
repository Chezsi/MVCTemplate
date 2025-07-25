// Date range filter using DataTables custom filter extension
$.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
    let min = $('#startDate').val();
    let max = $('#endDate').val();
    let dateStr = data[2]; // "validity" is column index 2

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

$('#nameSearch').on('keyup change', function () {
    dataTable.column(0).search(this.value).draw();
});

$('#descriptionSearch').on('keyup change', function () {
    dataTable.column(1).search(this.value).draw();
});

$('#startDate, #endDate').on('change', function () {
    const startVal = $('#startDate').val();
    const endVal = $('#endDate').val();

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const startDate = startVal ? new Date(startVal) : null;
    const endDate = endVal ? new Date(endVal) : null;

    if (startDate) startDate.setHours(0, 0, 0, 0);
    if (endDate) endDate.setHours(0, 0, 0, 0);

    const resetDates = (title, message) => {
        Swal.fire({
            icon: 'warning',
            title: title,
            text: message
        }).then(() => {
            $('#startDate').val('');
            $('#endDate').val('');
            dataTable.draw();
        });
        return;
    };

    if (startDate && startDate > today) {
        return resetDates('Invalid Start Date', 'Start date cannot be in the future.');
    }

    if (startDate && endDate && startDate > endDate) {
        return resetDates('Invalid Date Range', 'Start date cannot be after end date.');
    }

    if (endDate && endDate > today) {
        return resetDates('Invalid End Date', 'End date cannot be in the future.');
    }

    dataTable.draw();
});

$('#personIdSearch').on('change', function () {
    dataTable.column(3).search(this.value).draw();
});
