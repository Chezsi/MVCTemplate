$(document).ready(function () {
    // Add manager
    $('#addManagerForm').submit(function (e) {
        e.preventDefault();

        $.ajax({
            url: '/Admin/Manager/CreateFromSite',
            type: 'POST',
            data: $(this).serialize(),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    setTimeout(function () {
                        $('#addManagerModal').modal('hide');
                        location.reload();
                    }, 2000);
                } else {
                    toastr.error(res.message);
                }
            },
            error: function () {
                toastr.error("An unexpected error occurred.");
            }
        });
    });

    // Edit manager
    $('#editManagerForm').submit(function (e) {
        e.preventDefault();

        $.ajax({
            url: '/Admin/Manager/EditFromSite',
            type: 'POST',
            data: $(this).serialize(),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    $('#editManagerModal').modal('hide');
                    $('#siteManagersTable').DataTable().ajax.reload();
                } else {
                    toastr.error(res.message);
                }
            },
            error: function () {
                toastr.error('Failed to update manager.');
            }
        });
    });
});


$(document).on('click', '.btn-edit-manager', function () {
    const id = $(this).data('id');
    const name = $(this).data('name');
    const email = $(this).data('email');

    $('#editManagerId').val(id);
    $('#editManagerName').val(name);
    $('#editManagerEmail').val(email);
    $('#editManagerModal').modal('show');
});
$(document).on('click', '.delete-btn', async function () {
    const managerId = $(this).data('id');

    const result = await Swal.fire({
        title: 'Are you sure?',
        text: "This manager will be permanently deleted.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    });

    if (result.isConfirmed) {
        try {
            const response = await $.ajax({
                url: `/Admin/Manager/Delete/${managerId}`,
                type: 'DELETE'
            });

            $('#siteManagersTable').DataTable().ajax.reload();
            toastr.success(response.message);
        } catch (error) {
            toastr.error(error?.responseJSON?.message || "Something went wrong.");
        }
    }
});


