$(document).ready(function () {
    $(document).on('click', '.countRoleUsersBtn', function () {
        let btn = $(this)

        let roleId = $(this).val();
        let data = {
            roleId: roleId,
        };

        ajaxDataRequest('/api/security/role/count-users-by-role-id', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                // console.log('Received data:', data);
                btn.text(data.totalCount);
            }
        });
    });
});
