$(document).ready(function () {
    $('#button-to-excel-contract').on('click', function () {
        const btn = $(this);

        if (btn.prop('disabled')) return;

        btn.prop('disabled', true);
        const originalHtml = btn.html();
        btn.html('<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...');

        $.post('/Admin/Contract/GenerateDownloadToken')
            .done(function (response) {
                if (response.token) {
                    window.location.href = '/Admin/Contract/ExportToExcel?token=' + encodeURIComponent(response.token);

                    // Re-enable button after 1 second
                    setTimeout(() => {
                        btn.prop('disabled', false);
                        btn.html(originalHtml);
                    }, 1000);
                } else {
                    alert('Failed to get download token.');
                    btn.prop('disabled', false);
                    btn.html(originalHtml);
                }
            })
            .fail(function () {
                alert('Error generating download token.');
                btn.prop('disabled', false);
                btn.html(originalHtml);
            });
    });
});
// uses CS