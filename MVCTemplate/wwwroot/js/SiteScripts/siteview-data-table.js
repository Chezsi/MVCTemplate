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
                title: 'Updated At',
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
