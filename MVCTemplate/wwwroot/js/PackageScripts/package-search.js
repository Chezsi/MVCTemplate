// Filter by Name column
$('#nameSearch').on('keyup change', function () {
    dataTable.column(0).search(this.value).draw(); // 0 = "Name" column index
});

// Filter by Description column
$('#descriptionSearch').on('keyup change', function () {
    dataTable.column(1).search(this.value).draw();
});

// Filter by Priority column
$('#prioritySearch').on('keyup change', function () {
    let val = this.value.trim();
    const $error = $('#prioritySearchError');

    if (val === '' || (/^[1-5]$/).test(val)) {
        dataTable.column(2).search(val).draw();
        $error.text('').hide();
    } else {
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
        $('#button-excelFiltered-package').show();
    } else {
        $('#button-excelFiltered-package').hide();
    }
}

// Date range filtering (custom)
$.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
    let min = $('#startDate').val();
    let max = $('#endDate').val();
    let dateStr = data[3]; // Assuming createdAt is at column index 3

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

// Redraw on date change
$('#startDate, #endDate').on('change', function () {
    const startVal = $('#startDate').val();
    const endVal = $('#endDate').val();

    if (!startVal && !endVal) {
        dataTable.draw();
        toggleExportFilteredButton();
        return;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const startDate = startVal ? new Date(startVal) : null;
    const endDate = endVal ? new Date(endVal) : null;

    if (startDate) startDate.setHours(0, 0, 0, 0);
    if (endDate) endDate.setHours(0, 0, 0, 0);

    const resetAndExit = (title, message) => {
        Swal.fire({
            icon: 'warning',
            title: title,
            text: message,
        }).then(() => {
            $('#startDate').val('');
            $('#endDate').val('');
            dataTable.draw();
            toggleExportFilteredButton();
        });
        return true; 
    };

    if (startDate && endDate && startDate > endDate) {
        return resetAndExit('Invalid Date Range', 'Start date cannot be after end date.');
    }

    if (startDate && startDate > today) {
        return resetAndExit('Future Date Detected', 'Start date cannot be in the future.');
    }

    if (endDate && endDate > today) {
        return resetAndExit('Future Date Detected', 'End date cannot be in the future.');
    }

    dataTable.draw();
    toggleExportFilteredButton();
});

$(document).ready(function () {
    $('#button-excelFiltered-package').hide();

    $('#nameSearch, #descriptionSearch, #prioritySearch')
        .on('keyup change', toggleExportFilteredButton);

    $('#button-excelFiltered-package').on('click', function () {
        const btn = $(this);
        let filteredData = dataTable.rows({ search: 'applied' }).data().toArray();

        if (filteredData.length === 0) {
            alert("No data available to export.");
            return;
        }

        btn.prop('disabled', true);
        const originalHtml = btn.html();
        btn.html('<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...');

        // Step 1: Request token
        $.post('/Admin/Package/GenerateDownloadToken')
            .done(function (response) {
                if (response.token) {
                    // Step 2: Send filtered data with token as query param
                    $.ajax({
                        url: '/Admin/Package/ExportFilteredToExcel?token=' + encodeURIComponent(response.token),
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
                        },
                        complete: function () {
                            setTimeout(() => {
                                btn.prop('disabled', false);
                                btn.html(originalHtml);
                            }, 1000);
                        }
                    });
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
});
