$(document).ready(function () {
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
                    const date = new Date(data);
                    if (date.getFullYear() === 1 && date.getMonth() === 0 && date.getDate() === 1) {
                        return '';
                    }
                    return date.toLocaleString('en-US', {
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

            // ✅ Step 2: Add this hidden column
            { data: 'filePath', visible: false },

            {
                data: 'id',
                "render": function (data, type, full, meta) {
                    let buttons = `
                <div class="w-75 btn-group" role="group">
                    <button type="button" 
                        data-id="${data}" 
                        data-name="${full.name}" 
                        data-description="${full.description}" 
                        data-priority="${full.priority}" 
                        data-createdAt="${full.createdAt}" 
                        data-updatedAt="${full.updatedAt}" 
                        class="btn-shadow btn btn-info" 
                        data-bs-toggle="modal" 
                        data-bs-target="#updateModal">
                        <i class="lnr-pencil"></i> Edit
                    </button>`;

                    // ✅ Step 3: Add View button if filePath is present
                    if (full.filePath && full.filePath.trim() !== "") {
                        buttons += `
                    <a href="/${full.filePath}" target="_blank" class="btn-shadow btn btn-warning mx-2">
                        <i class="lnr-eye"></i> View
                    </a>`;
                    }

                    if (currentUserRole?.toLowerCase() === 'admin') {
                        buttons += `
                    <a onClick="Delete('/Admin/Package/Delete/${data}')" class="btn-shadow btn btn-danger mx-3">
                        <i class="lnr-trash"></i> Delete
                    </a>`;
                    }

                    buttons += `</div>`;
                    return buttons;
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
