function addTemporaryClass(button) {
    button.classList.add('expand-left');
    setTimeout(() => {
        button.classList.remove('expand-left');
    }, 300);
}

document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('ExportAllChartsBtn-product')?.addEventListener('click', function () {
        addTemporaryClass(this);
    });

    document.getElementById('button-to-excel-product')?.addEventListener('click', function () {
        addTemporaryClass(this);
    });

    document.getElementById('button-to-excel-category')?.addEventListener('click', function () {
        addTemporaryClass(this);
    });
    document.getElementById('button-to-excel-person')?.addEventListener('click', function () {
        addTemporaryClass(this);
    });
    document.getElementById('ExportAllChartsBtn-person')?.addEventListener('click', function () {
        addTemporaryClass(this);
    });
    document.getElementById('ExportAllChartsBtn-contract')?.addEventListener('click', function () {
        addTemporaryClass(this);
    });

    document.getElementById('button-to-excel-contract')?.addEventListener('click', function () {
        addTemporaryClass(this);
    });
});
