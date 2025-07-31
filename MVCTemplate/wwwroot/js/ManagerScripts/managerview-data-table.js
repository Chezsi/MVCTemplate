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
                render: data => !data ? '' : new Date(data).toLocaleDateString('en-US', {
                    year: 'numeric', month: 'long', day: '2-digit'
                })
            },
            {
                data: 'updatedAt',
                title: 'Updated',
                render: data => !data || data.includes('0001') ? '' : new Date(data).toLocaleDateString('en-US', {
                    year: 'numeric', month: 'long', day: '2-digit'
                })
            },
            {
                data: 'id',
                title: 'Action',
                orderable: false,
                render: function (id, type, row) {
                    return `
            <button class="btn btn-sm btn-warning btn-edit-product"
                    data-id="${id}"
                    data-name="${row.name}"
                    data-description="${row.description}"
                    data-quantity="${row.quantity}">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-danger delete-product-btn" data-id="${id}">
                <i class="fas fa-trash-alt"></i>
            </button>
        `;
                }
            }
        ]
    });
});
