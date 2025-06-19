document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('button-to-excel-report');
    if (!btn) return;

    const originalHtml = btn.innerHTML;

    btn.addEventListener('click', function (event) {
        if (btn.classList.contains('disabled')) {
            event.preventDefault();
            return;
        }

        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.innerHTML = `<i class="fa-solid fa-spinner fa-spin" style="color: #fff; margin-right: 6px;"></i> Exporting...`;

        setTimeout(() => {
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
            btn.innerHTML = originalHtml;
        }, 1000);
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('button-to-pdf-report');
    if (!btn) return;

    const originalHtml = btn.innerHTML;

    btn.addEventListener('click', function (event) {
        if (btn.classList.contains('disabled')) {
            event.preventDefault();
            return;
        }

        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.innerHTML = `<i class="fa-solid fa-spinner fa-spin" style="color: #fff; margin-right: 6px;"></i> Exporting...`;

        setTimeout(() => {
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
            btn.innerHTML = originalHtml;
        }, 1000);
    });
});
