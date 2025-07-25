let currentCategoryId = null;

$(document).ready(function () {
    loadDataTableCategory();
});

function loadDataTableCategory() {
    dataTable = $('#categoryTable').DataTable({
        "ajax": { url: '/Admin/Category/GetAllCategory' },
        "columns": [
            {
                data: 'nameCategory',
                render: function (data) {
                    return `<span class="text-truncate">${data}</span>`;
                },
                autowidth: true
            },
            { data: 'codeCategory', autowidth: true },
            {
                data: 'idCategory',
                render: function (data, type, full, meta) {
                    return `
                        <div class="w-100 btn-group" role="group">
                            <button 
                                type="button" 
                                class="btn btn-sm btn-secondary me-2" 
                                data-bs-toggle="modal" 
                                data-bs-target="#infoModal"
                                data-id="${full.idCategory}" 
                                data-name="${full.nameCategory}" 
                                data-code="${full.codeCategory}"
                                title="View Details">
                                <i class="fa fa-info-circle"></i> View
                            </button>
                            <button 
                                type="button" 
                                class="btn btn-sm btn-info me-2" 
                                data-id="${data}" 
                                data-name="${full.nameCategory}" 
                                data-code="${full.codeCategory}" 
                                data-bs-toggle="modal" 
                                data-bs-target="#updateModal"
                                title="Edit Category">
                                <i class="lnr-pencil"></i> Edit
                            </button>
                            <a 
                                onClick="Delete('/Admin/Category/Delete/${data}')" 
                                class="btn btn-sm btn-danger"
                                title="Delete Category">
                                <i class="lnr-trash"></i> Delete
                            </a>
                        </div>`;
                },
                width: "25%", className: "text-center", orderable: false
            }
        ]
    });
}


$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var idCategory = button.data('id');
    var nameCategory = button.data('name');
    var codeCategory = button.data('code');
    var modal = $(this);
    modal.find('.modal-body #idCategory').val(idCategory);
    modal.find('.modal-body #nameCategory').val(nameCategory);
    modal.find('.modal-body #codeCategory').val(codeCategory);
});

$('#infoModal').on('show.bs.modal', function (event) {
    const button = $(event.relatedTarget);
    const name = button.data('name');
    const code = button.data('code');
    const categoryId = button.data('id');

    currentCategoryId = categoryId;

    $('#infoName').text(name);
    $('#infoCode').text(code);

    $('#exportExcelBtn').attr('href', `/Admin/Person/ExportByCategory?categoryId=${categoryId}`);

    loadPersonsForCategory(categoryId);
});

function loadPersonsForCategory(categoryId) {
    $('#personListBody').html('<tr><td colspan="4">Loading...</td></tr>');

    $.ajax({
        url: `/Admin/Category/GetPersonsByCategory?categoryId=${categoryId}`,
        method: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        success: function (persons) {
            if (!persons.length) {
                $('#personListBody').html('<tr><td colspan="4" class="text-muted text-center">No persons found.</td></tr>');
            } else {
                const rows = persons.map(p => `
                    <tr>
                        <td>${p.name}</td>
                        <td>${p.position || '<i class="text-muted">None</i>'}</td>
                        <td>${p.createdAt}</td>
                        <td>
                            <div class="btn-group">
                                <button
                                    class="btn btn-sm btn-secondary me-2"
                                    onclick="openContractsModal(${p.id}, '${p.name}', '${p.position || ''}')">
                                    <i class="fa fa-file-contract"></i> View
                                </button>
                                <button 
                                    class="btn btn-sm btn-info me-2"
                                    onclick="openEditPersonModal(${p.id}, '${p.name}', '${p.position || ''}')">
                                    <i class="lnr-pencil"></i> Edit
                                </button>
                                <button 
                                    class="btn btn-sm btn-danger"
                                    onclick="deletePerson(${p.id})">
                                    <i class="lnr-trash"></i> Delete
                                </button>
                            </div>
                        </td>
                    </tr>
                `).join('');

                $('#personListBody').html(rows);
            }
        },
        error: function () {
            $('#personListBody').html('<tr><td colspan="4" class="text-danger">Error loading data.</td></tr>');
        }
    });
}

function openEditPersonModal(id, name, position) {
    $('#editPersonId').val(id);
    $('#editPersonName').val(name);
    $('#editPersonPosition').val(position || '');
    $('#editCategoryId').val(currentCategoryId);

    $('#editPersonModal').modal('show');
}

function submitEditPerson() {
    const id = $('#editPersonId').val();
    const name = $('#editPersonName').val().trim();
    const position = $('#editPersonPosition').val().trim();
    const categoryId = $('#editCategoryId').val();

    if (!name || !position || !categoryId) {
        alert("All fields are required.");
        return;
    }

    $.ajax({
        url: '/Admin/Person/UpdateViaJson',
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({ id, name, position, categoryId }),
        success: function (res) {
            $('#editPersonModal').modal('hide');
            toastr.success(res.message || "Updated successfully.");
            loadPersonsForCategory(categoryId);
        },
        error: function (err) {
            console.error("Update failed:", err.responseJSON?.errors || err.responseJSON || err);
            const errorDetails = err.responseJSON?.errors;
            if (errorDetails) {
                for (const field in errorDetails) {
                    errorDetails[field].forEach(msg => toastr.error(msg));
                }
            }
        }
    });
}

$(document).ready(function () {
    $('#editPersonForm').submit(function (e) {
        e.preventDefault(); // stop native form submit
        submitEditPerson(); // your AJAX function
    });
});

function openAddPersonModal() {
    $('#addPersonName').val('');
    $('#addPersonPosition').val('');
    $('#addPersonModal').modal('show');
}

function submitAddPerson() {
    const name = $('#addPersonName').val().trim();
    const position = $('#addPersonPosition').val().trim();
    const categoryId = currentCategoryId; // from modal context

    if (!name || !position || !categoryId) {
        alert("All fields are required.");
        return;
    }

    $.ajax({
        url: '/Admin/Person/CreateViaJson',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ name, position, categoryId }),
        success: function (res) {
            $('#addPersonModal').modal('hide');
            toastr.success(res.message || "Added successfully.");
            loadPersonsForCategory(categoryId); // refresh list
        },
        error: function (err) {
            console.error("Add person failed:", err.responseJSON?.errors || err.responseJSON?.message || err);
            const errorDetails = err.responseJSON?.errors;
            if (errorDetails) {
                for (const field in errorDetails) {
                    errorDetails[field].forEach(msg => toastr.error(msg));
                }
            }
        }
    });
}

$(document).ready(function () {
    $('#addPersonForm').submit(function (e) {
        e.preventDefault();
        submitAddPerson();
    });
});

function deletePerson(id) {
    if (confirm("Are you sure you want to delete this person?")) {
        $.ajax({
            url: `/Admin/Person/Delete/${id}`,
            type: 'DELETE',
            success: function (res) {
                alert(res.message);
                loadPersonsForCategory(currentCategoryId);
            },
            error: function (err) {
                alert(err.responseJSON?.message || "Delete failed.");
            }
        });
    }
}

function openContractsModal(personId, name, position) {
    $('#contractPersonInfo').html(`
        <p><strong>Name:</strong> ${name}</p>
        <p><strong>Position:</strong> ${position || '<i class="text-muted">None</i>'}</p>
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

    $('#contractsModal').modal('show');
}


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