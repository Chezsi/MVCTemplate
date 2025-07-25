$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#personTable').DataTable({
        "ajax": { url: '/Admin/Person/GetAllPersons' },
        "columns": [
            {
                data: 'name',
                render: function (data) {
                    return `<span class="text-truncate">${data}</span>`;
                },
                autowidth: true
            },
            { data: 'position', autowidth: true },
            { data: 'categoryId', autowidth: true },
            {
                data: 'id',
                render: function (data, type, full, meta) {
                    return `
                        <div class="btn-group" role="group">
                            <button type="button" 
                                class="btn btn-sm btn-outline-info"
                                data-bs-toggle="modal" 
                                data-bs-target="#infoModal" 
                                data-id="${data}" 
                                data-name="${full.name}" 
                                data-position="${full.position}" 
                                data-categoryid="${full.categoryId}">
                                <i class="fa fa-info-circle"></i> View
                            </button>

                            <button type="button" 
                                class="btn btn-sm btn-info ms-1" 
                                data-id="${data}" 
                                data-name="${full.name}" 
                                data-position="${full.position}" 
                                data-categoryid="${full.categoryId}" 
                                data-bs-toggle="modal" 
                                data-bs-target="#updateModal">
                                <i class="lnr-pencil"></i> Edit
                            </button>

                            <a onClick="Delete('/Admin/Person/Delete/${data}')" 
                                class="btn btn-sm btn-danger ms-1">
                                <i class="lnr-trash"></i> Delete
                            </a>
                        </div>`;
                },
                width: "25%", className: "text-center", orderable: false
            }
        ]
    });
}


// Update Modal
$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var id = button.data('id');
    var name = button.data('name');
    var position = button.data('position');
    var categoryId = button.data('categoryid');
    var modal = $(this);
    modal.find('.modal-body #id').val(id);
    modal.find('.modal-body #name').val(name);
    modal.find('.modal-body #position').val(position);
    modal.find('.modal-body #CategoryId').val(categoryId).trigger('change');
});

// Info Modal — CONTRACTS
$('#infoModal').on('show.bs.modal', function (event) {
    const button = $(event.relatedTarget);
    const name = button.data('name');
    const position = button.data('position');
    const personId = button.data('id'); // use data-id passed from render

    $('#personInfo').html(`
        <p><strong>Name:</strong> ${name}</p>
        <p><strong>Position:</strong> ${position}</p>
    `);

    $('#contractListBody').html('<tr><td colspan="4">Loading...</td></tr>');

    $.ajax({
        url: `/Admin/Person/GetContractsByPerson?personId=${personId}`,
        method: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (contracts) {
            if (contracts.length === 0) {
                $('#contractListBody').html('<tr><td colspan="4" class="text-muted text-center">No contracts found.</td></tr>');
            } else {
                const rows = contracts.map(c => `
                    <tr>
                        <td>${c.name}</td>
                        <td>${c.description}</td>
                        <td>${c.validity}</td>
                        <td>${c.createdAt}</td>
                    </tr>
                `).join('');
                $('#contractListBody').html(rows);
            }
        },
        error: function () {
            $('#contractListBody').html('<tr><td colspan="4" class="text-danger">Failed to load contracts.</td></tr>');
        }
    });
});