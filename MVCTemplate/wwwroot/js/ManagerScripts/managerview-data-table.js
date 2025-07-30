$(document).ready(function () {
    const urlParts = window.location.pathname.split("/");
    const managerId = urlParts[urlParts.length - 1];

    $('#managerProductsTable').DataTable({
        ajax: {
            url: `/Admin/Manager/GetByManager/${managerId}`,
            type: 'GET',
            dataSrc: 'data'
        },
        columns: [
            { data: 'name', title: 'Product Name' },
            { data: 'description', title: 'Description' },
            { data: 'quantity', title: 'Quantity' },
            { data: 'branch', title: 'Branch' },
            {
                data: 'createdAt',
                title: 'Created',
                render: function (data) {
                    if (!data) return '';
                    return new Date(data).toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'long',
                        day: '2-digit'
                    });
                }
            },
            {
                data: 'updatedAt',
                title: 'Updated',
                render: function (data) {
                    if (!data || data.includes('0001')) return '';
                    return new Date(data).toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'long',
                        day: '2-digit'
                    });
                }
            }
        ]
    });
});
