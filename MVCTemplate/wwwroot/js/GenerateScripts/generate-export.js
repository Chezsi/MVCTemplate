document.addEventListener('DOMContentLoaded', function () {
    const btn = document.getElementById('button-pdf');
    if (!btn) return; // safety check

    const originalHtml = btn.innerHTML;

    btn.addEventListener('click', function (event) {
        if (btn.classList.contains('disabled')) {
            event.preventDefault();
            return;
        }

        btn.classList.add('disabled');
        btn.style.pointerEvents = 'none';
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="color: #fff; margin-right: 6px;"></i> Exporting...';

        setTimeout(() => {
            btn.classList.remove('disabled');
            btn.style.pointerEvents = 'auto';
            btn.innerHTML = originalHtml;
        }, 1000);
    });
});
// somehow slower than exports outside of generate

document.addEventListener('DOMContentLoaded', function () {
    const btnExcel = document.getElementById('button-excel');
    if (!btnExcel) return; // safety check

    const originalHtml = btnExcel.innerHTML;

    btnExcel.addEventListener('click', function (event) {
        if (btnExcel.classList.contains('disabled')) {
            event.preventDefault();
            return;
        }

        btnExcel.classList.add('disabled');
        btnExcel.style.pointerEvents = 'none';
        btnExcel.innerHTML = '<i class="fa-solid fa-spinner fa-spin" style="color: #fff; margin-right: 6px;"></i> Exporting...';

        setTimeout(() => {
            btnExcel.classList.remove('disabled');
            btnExcel.style.pointerEvents = 'auto';
            btnExcel.innerHTML = originalHtml;
        }, 1000);
    });
});
