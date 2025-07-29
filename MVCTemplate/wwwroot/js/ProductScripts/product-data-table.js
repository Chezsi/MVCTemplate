
// Custom filter to handle min/max quantity filtering
$.fn.dataTable.ext.search.push(
    function (settings, data, dataIndex) {
        var min = parseInt($('#minQuantity').val(), 10);
        var max = parseInt($('#maxQuantity').val(), 10);
        var quantity = parseInt(data[2]) || 0;

        if ((isNaN(min) && isNaN(max)) ||
            (isNaN(min) && quantity <= max) ||
            (min <= quantity && isNaN(max)) ||
            (min <= quantity && quantity <= max)) {
            return true;
        }
        return false;
    }
);

$(document).ready(function () {
    loadDataTable();

});


// Step 2: Define the DataTable and rendering logic

function loadDataTable() {
    dataTable = $('#productTable').DataTable({
        "ajax": { url: '/Admin/Product/GetAllProducts' },
        "columns": [
            { data: 'name', "autowidth": true },
            { data: 'description', "autowidth": true },
            { data: 'quantity', "autowidth": true },
            { data: 'managerName', title: 'Manager', autowidth: true },
            {
                data: 'id',
                "render": function (data, type, full, meta) {
                    let buttons = `
                        <div class="w-75 btn-group" role="group">
                            <button type="button"
                                data-id="${data}"
                                data-name="${full.name}"
                                data-description="${full.description}"
                                data-quantity="${full.quantity}"
                                data-managerid="${full.managerId}"
                                class="btn-shadow btn btn-info"
                                data-bs-toggle="modal"
                                data-bs-target="#updateModal">
                                <i class="lnr-pencil"></i> Edit
                            </button>
                            `;

                    if ((currentUserRole || "").toLowerCase() === 'admin') {
                        buttons += `
                            <a href="javascript:void(0);" onClick="Delete('/Admin/Product/Delete/${data}')" class="btn-shadow btn btn-danger mx-3">
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
}


$('#nameSearch').on('keyup change', function () {
    dataTable.column(0).search(this.value).draw(); // 0 is the index for the "Name" column
});

// Filter by Description column
$('#descriptionSearch').on('keyup change', function () {
    dataTable.column(1).search(this.value).draw();
});

// min and max quantity filtering
$('#minQuantity, #maxQuantity').on('input', function () {
    // Remove anything that's not a digit
    this.value = this.value.replace(/[^0-9]/g, '');
    // Trigger table redraw
    dataTable.draw();
});

// Call loadDataTable() to initialize your DataTable when the page loads

$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var id = button.data('id');
    var name = button.data('name');
    var description = button.data('description');
    var quantity = button.data('quantity');
    var managerId = button.data('managerid');

    var modal = $(this);
    modal.find('.modal-body #productId').val(id);
    modal.find('.modal-body #name').val(name);
    modal.find('.modal-body #description').val(description);
    modal.find('.modal-body #quantity').val(quantity);
    modal.find('.modal-body #ManagerId').val(managerId).trigger('change');
});

