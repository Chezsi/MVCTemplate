let dataTable;

$(document).ready(function () {
    loadDataTable();

    // Load person filter dropdown separately
    $.get('/Admin/Contract/GetAllPersonsForContract', function (data) {
        const personFilter = $('#personIdSearch');
        personFilter.empty();
        personFilter.append($('<option>').val('').text('-- All People --'));

        data.forEach(person => {
            personFilter.append($('<option>').val(person.name).text(person.name));
        });
    });

    // Unlock form submit handler
    $('#unlockForm').on('submit', function (e) {
        e.preventDefault();

        const contractId = $('#unlockContractId').val();
        const key = $('#unlockKey').val().trim();

        if (!key) {
            $('#unlockError').text("Unlock key cannot be empty.").show();
            return;
        }

        $.ajax({
            url: `/Admin/Contract/Unlock/${contractId}`,
            method: 'POST',
            data: { key: key },
            success: function (response) {
                $('#unlockModal').modal('hide');
                toastr.success(response.message || "Contract unlocked successfully.");
                dataTable.ajax.reload(null, false);
            },
            error: function (xhr) {
                const errMsg = xhr.responseJSON?.message || "Failed to unlock contract.";
                $('#unlockError').text(errMsg).show();
            }
        });
    });

    // Initialize the DataTable
    function loadDataTable() {
        dataTable = $('#contractTable').DataTable({
            "ajax": { url: '/Admin/Contract/GetAllContracts' },
            "columns": [
                { data: 'name', "autowidth": true },
                { data: 'description', "autowidth": true },
                {
                    data: 'validity',
                    render: function (data) {
                        if (!data) return '';
                        const date = new Date(data);
                        return date.toLocaleDateString('en-US', {
                            year: 'numeric', month: 'long', day: 'numeric'
                        });
                    },
                    "autowidth": true
                },
                { data: 'personName', "autowidth": true },
                {
                    data: 'id',
                    render: function (data, type, full) {
                        const validityDate = new Date(full.validity);
                        const today = new Date();
                        today.setHours(0, 0, 0, 0);

                        const isEditable = validityDate >= today;

                        const editButton = isEditable
                            ? `<button type="button"
                                data-id="${data}"
                                data-name="${full.name}"
                                data-description="${full.description}"
                                data-validity="${full.validity}"
                                data-person-id="${full.personId}"
                                class="btn-shadow btn btn-info"
                                data-bs-toggle="modal"
                                data-bs-target="#updateModal">
                                <i class="lnr-pencil"></i> Edit
                            </button>`
                            : `<button type="button"
                                class="btn-shadow btn btn-warning unlock-btn"
                                data-id="${data}"
                                title="Click to unlock this contract">
                                <i class="lnr-lock"></i> Locked
                            </button>`;

                        const deleteButton = `<a href="javascript:void(0);" onClick="Delete('/Admin/Contract/Delete/${data}')" class="btn-shadow btn btn-danger mx-3">
                            <i class="lnr-trash"></i> Delete
                        </a>`;

                        return `<div class="w-75 btn-group" role="group">
                            ${editButton}
                            ${deleteButton}
                        </div>`;
                    },
                    width: "25%", className: "text-center", orderable: false
                }
            ]
        });
    }

    // Unlock button click handler (delegated)
    $('#contractTable tbody').on('click', '.unlock-btn', function () {
        const contractId = $(this).data('id');
        $('#unlockContractId').val(contractId);
        $('#unlockKey').val('');
        $('#unlockError').hide();
        $('#unlockModal').modal('show');
    });

    // Populate update modal
    $('#updateModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var id = button.data('id');
        var name = button.data('name');
        var description = button.data('description');
        var validity = button.data('validity');
        var personId = button.data('personId');
        var modal = $(this);

        modal.find('#Contract_Id').val(id);
        modal.find('#Contract_Name').val(name);
        modal.find('#Contract_Description').val(description);
        modal.find('#Contract_Validity').val(validity ? validity.split('T')[0] : '');

        const personSelect = modal.find('#Contract_PersonId');

        if (personSelect.find(`option[value="${personId}"]`).length === 0 && personId) {
            $.get(`/Admin/Contract/GetPersonNameById?id=${personId}`, function (response) {
                personSelect.append(new Option(response.name, personId, true, true)).trigger('change');
            }).fail(function () {
                personSelect.append(new Option(`(Unknown Person #${personId})`, personId, true, true)).trigger('change');
            });
        } else {
            personSelect.val(personId).trigger('change');
        }
    });
});
