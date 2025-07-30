$(document).ready(function () {
    const urlParts = window.location.pathname.split("/");
    const siteId = urlParts[urlParts.length - 1];

    $('#siteManagersTable').DataTable({
        ajax: {
            url: `/Admin/Site/GetManagersBySite/${siteId}`,
            type: 'GET',
            dataSrc: 'data'
        },
        columns: [
            { data: 'name', title: 'Name' },
            { data: 'email', title: 'Email' },
            {
                data: 'createdAt',
                title: 'Created At',
                render: function (data) {
                    return new Date(data).toLocaleDateString('en-US', {
                        year: 'numeric', month: 'long', day: '2-digit'
                    });
                }
            },
            {
                data: 'updatedAt',
                title: 'Updated At',
                render: function (data) {
                    return !data || data.includes('0001') ? '' :
                        new Date(data).toLocaleDateString('en-US', {
                            year: 'numeric', month: 'long', day: '2-digit'
                        });
                }
            },
            {
                data: 'id',
                title: 'Action',
                orderable: false,
                render: function (id, type, row) {
                    return `
                    <button class="btn btn-sm btn-warning btn-edit-manager"
                            data-id="${id}" data-name="${row.name}" data-email="${row.email}">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-danger delete-btn" data-id="${id}">
                        <i class="fas fa-trash-alt"></i>
                    </button>
                `;
                }
            }
        ]
    });

});
