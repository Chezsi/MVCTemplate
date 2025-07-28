$(document).ready(function () {
    $('#accountTable').DataTable({
        ajax: {
            url: '/Admin/Account/GetAllUsers',
            type: 'GET',
            datatype: 'json',
            dataSrc: function (json) {
                console.log("Fetched user list:", json);
                return json.data;
            }
        },
        columns: [
            { data: 'email', width: '40%' },
            { data: 'role', width: '30%' },
            {
                data: null,
                render: function () {
                    return ''; // For future edit/delete buttons
                },
                orderable: false,
                width: '30%'
            }
        ],
        language: {
            emptyTable: 'No users found.'
        },
        width: '100%'
    });
});
