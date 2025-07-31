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
