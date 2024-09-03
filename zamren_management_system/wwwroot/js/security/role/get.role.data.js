$(document).ready(function () {

    window.getRoleData = function () {

        //get userId from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/role/get-role-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('.roleName').text(data.role.name);
                $('input[name="name"]').val(data.role.name);
                $('textarea[name="description"]').val(data.role.description);
                $('input[name="id"]').val(data.role.id);
                $('input[name="currentRoleId"]').val(data.role.id);
                $('.edit-role-link').attr('href', '/Admin/Role/Edit/' + data.role.id);
            }
        });
    };

    // Call the method on page load
    getRoleData();
});
