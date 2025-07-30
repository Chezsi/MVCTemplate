$(document).ready(function () {
    $('#addManagerForm').submit(function (e) {
        e.preventDefault();

        $.ajax({
            url: '/Admin/Manager/CreateFromSite',
            type: 'POST',
            data: $(this).serialize(),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);

                    // Delay reload so user sees the message
                    setTimeout(function () {
                        $('#addManagerModal').modal('hide');
                        location.reload(); // or refresh DataTable here instead
                    }, 2000); // delay 1.2 seconds
                } else {
                    toastr.error(res.message);
                }
            },
            error: function () {
                toastr.error("An unexpected error occurred.");
            }
        });
    });
});
