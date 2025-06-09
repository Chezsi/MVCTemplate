$(document).ready(function () {
    loadDataTableReport();

    // Click to enlarge one image at a time
    $(document).on('click', function (e) {
        if ($(e.target).hasClass('report-thumbnail')) {
            // Remove enlarged class from all first
            $('.report-thumbnail.enlarged').not(e.target).removeClass('enlarged');

            // Toggle the clicked image
            $(e.target).toggleClass('enlarged');
        } else {
            // Clicked outside any image
            $('.report-thumbnail.enlarged').removeClass('enlarged');
        }
    });

function loadDataTableReport() {
    dataTable = $('#reportTable').DataTable({
        "ajax": {
            url: '/Admin/Report/GetAllReports',
            type: 'GET',
            datatype: 'json'
        },
        "columns": [
            { data: 'title', autoWidth: true },
            {
                data: 'imageName',
                autoWidth: true,
                render: function (data, type, full, meta) {
                    if (data) {
                        return `<img src="/uploads/reports/${data}" alt="${full.title}" class="report-thumbnail" />`;
                    } else {
                        return 'No Image';
                    }
                }
            },
            { data: 'description', autoWidth: true },
            {
                data: 'id',
                render: function (data, type, full, meta) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <button type="button"
                                    data-id="${data}"
                                    data-title="${full.title}"
                                    data-imageName="${full.imageName}"
                                    data-description="${full.description}"
                                    class="btn-shadow btn btn-info"
                                    data-bs-toggle="modal"
                                    data-bs-target="#updateModal">
                                <i class="lnr-pencil"></i> Edit
                            </button>
                            <a href="javascript:void(0);" onClick="Delete('/Admin/Report/Delete/${data}')" class="btn-shadow btn btn-danger mx-3">
                                <i class="lnr-trash"></i> Delete
                            </a>
                        </div>`;
                },
                width: "25%",
                className: "text-center",
                orderable: false
            }
        ],
        "language": {
            "emptyTable": "No reports found."
        },
        "responsive": true,
        "autoWidth": false
    });
}

// Populate Update Modal when triggered
$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var id = button.data('id');
    var title = button.data('title');
    var imageName = button.data('imagename');
    var description = button.data('description');
    var modal = $(this);

    modal.find('#updateReportId').val(id);
    modal.find('#updateTitle').val(title);
    modal.find('#updateImageName').val(imageName);
    modal.find('#updateDescription').val(description);
});
