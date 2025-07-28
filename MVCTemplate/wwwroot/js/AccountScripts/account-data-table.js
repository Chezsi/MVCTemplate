$(document).ready(function () {
    $('#accountTable').DataTable({
        ajax: {
            url: '/Admin/Account/GetAllUsers',
            type: 'GET',
            datatype: 'json',
            dataSrc: function (json) {
                return json.data;
            }
        },
        columns: [
            { data: 'email', width: '40%' },
            { data: 'role', width: '30%' },
            {
                data: null,
                render: function (data, type, row) {
                    return `
                        <button class="btn btn-sm btn-primary edit-btn" 
                            data-email="${row.email}" 
                            data-role="${row.role}" 
                            data-bs-toggle="modal" 
                            data-bs-target="#editUserModal">
                            <i class="fa fa-edit"></i> Edit
                        </button>`;
                },
                orderable: false,
                width: '30%'
            }
        ],
        language: {
            emptyTable: 'No users found.'
        },
        width: '100%'
    });

    // Handle click on edit button
    $('#accountTable').on('click', '.edit-btn', function () {
        const email = $(this).data('email');
        const role = $(this).data('role');

        $('#editEmail').val(email);
        $('#editRole').val(role);
        $('#editOriginalEmail').val(email); // hidden input to track original
    });
});

$('#editUserForm').on('submit', function (e) {
    e.preventDefault();

    const form = $(this);
    const formData = form.serialize();

    $.post('/Admin/Account/Edit', formData)
        .done(function (res) {
            if (res.success) {
                $('#editUserError').addClass('d-none');
                $('#editUserSuccess').removeClass('d-none').text(res.message);
                setTimeout(() => location.reload(), 1000);
            } else {
                $('#editUserError').removeClass('d-none').text(res.errors.join(', '));
                $('#editUserSuccess').addClass('d-none');
            }
        })
        .fail(function () {
            $('#editUserError').removeClass('d-none').text('Something went wrong.');
            $('#editUserSuccess').addClass('d-none');
        });
});
