﻿$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#packageTable').DataTable({
        "ajax": { url: '/Admin/Package/GetAllPackages' },
        "columns": [
            { data: 'name', "autowidth": true },
            { data: 'description', "autowidth": true },
            { data: 'priority', "autowidth": true },
            {
                data: 'createdAt',
                "render": function (data) {
                    return new Date(data).toLocaleString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                        hour: 'numeric',
                        minute: '2-digit',
                        hour12: true
                    });
                },
                "autowidth": true
            },
            {
                data: 'updatedAt',
                "render": function (data) {
                    return new Date(data).toLocaleString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                        hour: 'numeric',
                        minute: '2-digit',
                        hour12: true
                    });
                },
                "autowidth": true
            },
            {
                data: 'id',
                "render": function (data, type, full, meta) {
                    return `<div class="w-75 btn-group" role="group">
                        <button type="button" data-id="${data}" data-name="${full.name}" data-description="${full.description}" data-priority="${full.priority}" data-createdAt="${full.createdAt}" data-updatedAt="${full.updatedAt}" class="btn-shadow btn btn-info" data-bs-toggle="modal" data-bs-target="#updateModal">
                            <i class="lnr-pencil"></i> Edit
                        </button>
                        <a onClick="Delete('/Admin/Package/Delete/${data}')" class="btn-shadow btn btn-danger mx-3">
                            <i class="lnr-trash"></i> Delete
                        </a>
                    </div>`;
                },
                width: "25%", className: "text-center", orderable: false
            }
        ]
    });
};

// Populate update modal with selected row data
$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var id = button.data('id');
    var name = button.data('name');
    var description = button.data('description');
    var priority = button.data('priority');
    var modal = $(this);
    modal.find('.modal-body #id').val(id);
    modal.find('.modal-body #name').val(name);
    modal.find('.modal-body #description').val(description);
    modal.find('.modal-body #priority').val(priority);
});
