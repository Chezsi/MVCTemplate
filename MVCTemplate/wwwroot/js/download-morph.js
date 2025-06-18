function addTemporaryClass(button) {
    button.classList.add('expand-left');
    setTimeout(() => {
        button.classList.remove('expand-left');
    }, 300);
}

document.addEventListener('DOMContentLoaded', () => {
    // List all button IDs to attach the animation
    const buttonIds = [
        'ExportAllChartsBtn-product',
        'button-to-excel-product',
        'button-to-excel-category',
        'button-to-excel-person',
        'ExportAllChartsBtn-person',
        'ExportAllChartsBtn-contract',
        'button-to-excel-contract',
        'exportContractsBtn',
        'ExportAllChartsBtn-package',
        'button-excelFiltered-package',
        'button-to-excel-package',
        'button-to-excel-report',
        'button-excelFiltered-report',
        'button-to-pdf-report',
        'button-pdfFiltered-report',
        'ExportAllChartsBtn',
        'button-excel',
        'button-pdf'
    ];

    buttonIds.forEach(id => {
        const btn = document.getElementById(id);
        if (btn) {
            btn.addEventListener('click', function () {
                addTemporaryClass(this);
            });
        }
    });
});
