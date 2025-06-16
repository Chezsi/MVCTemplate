$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#personTable').DataTable({ // ensure naming consistency
        "ajax": { url: '/Admin/Person/GetAllPersons' },
        "columns": [
            { data: 'name', "autowidth": true },
            { data: 'position', "autowidth": true },
            { data: 'categoryId', "autowidth": true }, //CHECK THE NAME IN GETALLPERSONS
            {
                data: 'id',
                "render": function (data, type, full, meta) {
                    return `<div class="w-75 btn-group" role="group">                                                                               
                                    <button type="button" data-id="${data}" data-name="${full.name}" data-position="${full.position}" data-categoryid="${full.categoryId}" class="btn-shadow btn btn-info" data-bs-toggle="modal" data-bs-target="#updateModal"> <i class="lnr-pencil"></i> Edit</button>
                                    <a onClick="Delete('/Admin/Person/Delete/${data}')" class="btn-shadow btn btn-danger mx-3"> <i class="lnr-trash"></i> Delete</a>
                                </div>`;
                },
                width: "25%", className: "text-center", orderable: false
            }
        ]
    });
}

$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var id = button.data('id'); // must be same name with data-
    var name = button.data('name');
    var position = button.data('position');
    var categoryId = button.data('categoryid'); // for foreign key
    var modal = $(this);
    modal.find('.modal-body #id').val(id);
    modal.find('.modal-body #name').val(name);
    modal.find('.modal-body #position').val(position);
    modal.find('.modal-body #CategoryId').val(categoryId).trigger('change');
    //console.log(categoryId)// for foreign key
});
