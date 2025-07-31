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

$(document).on('click', '.delete-product-btn', function () {
    const id = $(this).data('id');
    if (!confirm("Are you sure you want to delete this product?")) return;

    $.ajax({
        url: `/Admin/Product/Delete?id=${id}`,
        type: 'DELETE',
        success: function (res) {
            alert(res.message);
            $('#managerProductsTable').DataTable().ajax.reload();
        },
        error: function (xhr) {
            const msg = xhr.responseJSON?.message || 'Error deleting product';
            alert(msg);
        }
    });
});
