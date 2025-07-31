let userTable; // Global reference

$(document).ready(function () {
    // Initialize and assign to global variable
    userTable = $('#accountTable').DataTable({
        ajax: {
            url: '/Admin/Account/GetAllUsers',
            type: 'GET',
            datatype: 'json',
            dataSrc: 'data'
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

    // Populate edit modal fields
    $('#accountTable').on('click', '.edit-btn', function () {
        const email = $(this).data('email');
        const role = $(this).data('role');

        $('#editEmail').val(email);
        $('#editRole').val(role);
        $('#editOriginalEmail').val(email); // hidden input to track original
        $('#editPassword').prop('disabled', true).val('').attr('type', 'password');
        $('#togglePassword').prop('disabled', true).find('i').removeClass('fa-eye-slash').addClass('fa-eye');
        $('#toggleEnablePassword').find('i').removeClass('fa-unlock').addClass('fa-lock');
    });

    // Create user form submission
    $('#createUserForm').on('submit', function (e) {
        e.preventDefault();

        const form = $(this);
        const formData = form.serialize();
        const actionUrl = form.attr('action');

        $.post(actionUrl, formData)
            .done(function (res) {
                if (res.success) {
                    toastr.success(res.message, 'User Created');
                    setTimeout(() => {
                        $('#createUserModal').modal('hide');
                        form[0].reset();
                        userTable.ajax.reload(null, false);
                    }, 100);
                } else {
                    toastr.error(res.errors.join(', '), 'Create Failed');
                }
            })
            .fail(function () {
                toastr.error('Something went wrong. Please try again.', 'Error');
            });
    });
});

// Toggle password field enable/disable
$(document).on('click', '#toggleEnablePassword', function () {
    const passwordInput = $('#editPassword');
    const visibilityBtn = $('#togglePassword');
    const lockIcon = $(this).find('i');
    const visibilityIcon = visibilityBtn.find('i');

    const isDisabled = passwordInput.prop('disabled');

    if (isDisabled) {
        passwordInput.prop('disabled', false);
        visibilityBtn.prop('disabled', false);
    } else {
        passwordInput.prop('disabled', true).val('').attr('type', 'password');
        visibilityBtn.prop('disabled', true);
        visibilityIcon.removeClass('fa-eye-slash').addClass('fa-eye');
    }

    lockIcon.toggleClass('fa-lock fa-unlock');
});

// Edit user form submission
$('#editUserForm').on('submit', function (e) {
    e.preventDefault();

    const form = $(this);
    const formData = form.serialize();

    $.post('/Admin/Account/Edit', formData)
        .done(function (res) {
            if (res.success) {
                toastr.success(res.message, 'User Updated');
                setTimeout(() => {
                    $('#editUserModal').modal('hide');
                    form[0].reset();
                    userTable.ajax.reload(null, false);
                }, 100);
            } else {
                toastr.error(res.errors.join(', '), 'Update Failed');
            }
        })
        .fail(function () {
            toastr.error('Something went wrong. Please try again.', 'Error');
        });
});
