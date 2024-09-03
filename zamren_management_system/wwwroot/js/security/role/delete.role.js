$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteRoleBtn', function () {

        let btn = $(this);
        let roleName = btn.data('rolename');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this role: " + roleName + "?");
        if (!confirmation) {
            return;
        }

        let roleId = btn.data('roleid');

        let data = {
            roleId: roleId,
        };

        ajaxDataRequest("/api/security/role/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                userRolesDatatable.ajax.reload();
            }
        });

    });

});