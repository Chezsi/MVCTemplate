$(document).ready(function () {
    loadDataTableCategory();
});


function loadDataTableCategory() {
    dataTable = $('#categoryTable').DataTable({ // ensure naming consistency
        "ajax": { url: '/Admin/Category/GetAllCategory' },
        "columns": [
            { data: 'nameCategory', "autowidth": true },
            { data: 'codeCategory', "autowidth": true },
            {
                data: 'idCategory',
                "render": function (data, type, full, meta) {
                    return `<div class="w-75 btn-group" role="group">
                                    <button type="button" data-id="${data}" data-name="${full.nameCategory}" data-code="${full.codeCategory}" class="btn-shadow btn btn-info" data-bs-toggle="modal" data-bs-target="#updateModal"> <i class="lnr-pencil"></i> Edit</button>
                                    <a onClick="Delete('/Admin/Category/Delete/${data}')" class="btn-shadow btn btn-danger mx-3"> <i class="lnr-trash"></i> Delete</a>
                                </div>`;
                },
                width: "25%", className: "text-center", orderable: false
            }
        ]
    });
}

$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var idCategory = button.data('id'); // must be same name with data-
    var nameCategory = button.data('name');
    var codeCategory = button.data('code');
    var modal = $(this);
    modal.find('.modal-body #idCategory').val(idCategory);
    modal.find('.modal-body #nameCategory').val(nameCategory);
    modal.find('.modal-body #codeCategory').val(codeCategory);
});

/*
document.querySelector("#button-excel").addEventListener("click", async function () {
    var table = $('#Categorys').DataTable(); // ??change to account for the name of the class (Model)
    var searchValue = table.search();
    var dataToExport;

    if (searchValue) {
        dataToExport = table.rows({ search: 'applied' }).data().toArray();
    } else {
        let response = await fetch('/Admin/Category/GetAllCategory'); //change to account for url naming
        let result = await response.json();
        dataToExport = result.data;
    }

    dataToExport.sort((a, b) => a.name.localeCompare(b.name));

    let tempTable = document.createElement('table');
    let thead = document.createElement('thead');
    thead.innerHTML = '<tr><th>ID</th> <th>Name</th> <th>Code</th></tr>';
    tempTable.appendChild(thead); 

    let tbody = document.createElement('tbody');
    dataToExport.forEach(row => {
        let tr = document.createElement('tr');
        tr.innerHTML = `<td>${row.idcategory}</td> <td>${row.namecategory}</td> <td>${row.descriptioncategory}</td> <td>${row.quantity}</td>`;
        tbody.appendChild(tr); // ^ change to account for name of data (Model) and number of column 
    });
    tempTable.appendChild(tbody);

    TableToExcel.convert(tempTable, { name: "Category.xlsx" });
});*/