// External filters
$('#nameSearch').on('keyup change', function () {
    dataTable.column(0).search(this.value).draw(); // Name column
});

$('#codeSearch').on('keyup change', function () {
    dataTable.column(1).search(this.value).draw(); // Code column
});