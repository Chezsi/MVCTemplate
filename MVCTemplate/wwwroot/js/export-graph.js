document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.export-trigger').forEach(button => {
        button.addEventListener('click', function (e) {
            if (button.disabled) {
                e.preventDefault(); // prevent repeated click
                return;
            }

            // Disable the button & show spinner
            button.disabled = true;
            const originalHtml = button.innerHTML;
            button.innerHTML = `<i class="fa-solid fa-spinner fa-spin" style="margin-right: 6px;"></i> Exporting...`;

            // Let the browser follow the href (download)
            setTimeout(() => {
                button.disabled = false;
                button.innerHTML = originalHtml;
            }, 1000); // UI reset after 1 second
        });
    });
});
