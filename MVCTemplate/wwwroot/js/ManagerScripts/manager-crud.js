$(document).ready(function () {
    $('#addProductForm').submit(function (e) {
        e.preventDefault();

        $.ajax({
            url: '/Admin/Product/Create',
            type: 'POST',
            data: $(this).serialize(),
            success: function (res) {
                toastr.success(res.message);
                $('#addProductModal').modal('hide');
                $('#managerProductsTable').DataTable().ajax.reload();
            },
            error: function (xhr) {
                const response = xhr.responseJSON;
                if (response?.errors) {
                    const messages = Object.values(response.errors).flat();
                    toastr.error(messages.join("<br/>"));
                } else {
                    toastr.error(response?.message || "An error occurred.");
                }
            }
        });
    });
});

$(document).on('click', '.delete-product-btn', async function () {
    const id = $(this).data('id');
    const url = `/Admin/Product/Delete?id=${id}`;

    const result = await Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    });

    if (result.isConfirmed) {
        try {
            const response = await $.ajax({
                url: url,
                type: 'DELETE'
            });

            $('#managerProductsTable').DataTable().ajax.reload();
            toastr.success(response.message);
        } catch (error) {
            toastr.error(error?.responseJSON?.message || 'Error deleting product.');
        }
    }
});

$(document).on('click', '.btn-edit-product', function () {
    const id = $(this).data('id');
    const name = $(this).data('name');
    const description = $(this).data('description');
    const quantity = $(this).data('quantity');

    $('#editProductId').val(id);
    $('#editProductName').val(name);
    $('#editProductDescription').val(description);
    $('#editProductQuantity').val(quantity);
    $('#editProductModal').modal('show');
});

$('#editProductForm').submit(function (e) {
    e.preventDefault();

    $.ajax({
        url: '/Admin/Product/Update',
        type: 'PUT',
        data: $(this).serialize(),
        success: function (res) {
            toastr.success(res.message);
            $('#editProductModal').modal('hide');
            $('#managerProductsTable').DataTable().ajax.reload();
        },
        error: function (xhr) {
            const msg = xhr.responseJSON?.message || 'Failed to update product.';
            toastr.error(msg);
        }
    });
});
