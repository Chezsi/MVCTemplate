// Filter by Name column
$('#nameSearch').on('keyup change', function () {
    dataTable.column(0).search(this.value).draw(); // 0 is the index for the "Name" column
});

// Filter by Description column
$('#descriptionSearch').on('keyup change', function () {
    dataTable.column(1).search(this.value).draw();
});

$('#prioritySearch').on('keyup change', function () {
    let val = this.value.trim();
    const $error = $('#prioritySearchError'); // must match HTML

    if (val === '' || (/^[1-5]$/).test(val)) {
        // Valid input
        dataTable.column(2).search(val).draw();
        $error.text('').hide();
    } else {
        // Invalid input
        dataTable.column(2).search('').draw();
        $error.text('Please enter a number between 1 and 5.').show();
    }
});

function toggleExportFilteredButton() {
    const name = $('#nameSearch').val().trim();
    const description = $('#descriptionSearch').val().trim();
    const priority = $('#prioritySearch').val().trim();
    const startDate = $('#startDate').val();
    const endDate = $('#endDate').val();

    const filteredRowsCount = dataTable.rows({ search: 'applied' }).data().length;

    if ((name || description || priority || startDate || endDate) && filteredRowsCount > 0) {
        $('#button-excelFiltered').show();
    } else {
        $('#button-excelFiltered').hide();
    }
}

$(document).ready(function () {
    $('#button-excelFiltered').hide();

    $('#nameSearch, #descriptionSearch, #prioritySearch, #startDate, #endDate')
        .on('keyup change', toggleExportFilteredButton);
});


// Date range filter using DataTables custom filter extension
$.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
    let min = $('#startDate').val();
    let max = $('#endDate').val();
    let dateStr = data[3]; // "createdAt" is column index 3

    if (!dateStr) return false;

    let createdDate = new Date(dateStr);
    createdDate.setHours(0, 0, 0, 0); // Normalize time to 00:00 for consistency

    let minDate = min ? new Date(min) : null;
    let maxDate = max ? new Date(max) : null;

    if (minDate) minDate.setHours(0, 0, 0, 0);
    if (maxDate) maxDate.setHours(23, 59, 59, 999); // Include full end date

    if (
        (!minDate || createdDate >= minDate) &&
        (!maxDate || createdDate <= maxDate)
    ) {
        return true;
    }

    return false;
});


// Redraw table on date input change
$('#startDate, #endDate').on('change', function () {
    dataTable.draw();
});

$(document).ready(function () {
    $('#button-excelFiltered').on('click', function () {
        let filteredData = dataTable.rows({ search: 'applied' }).data().toArray();

        if (filteredData.length === 0) {
            alert("No data available to export.");
            return;
        }

        $.ajax({
            url: '/Admin/Package/ExportFilteredToExcel',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(filteredData),
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data, status, xhr) {
                const blob = new Blob([data], { type: xhr.getResponseHeader('Content-Type') });
                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = "FilteredPackages.xlsx";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                alert("Failed to export filtered data.");
            }
        });
    });
});