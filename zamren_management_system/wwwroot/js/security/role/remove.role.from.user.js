$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.removeRoleFromUserBtn', function () {

        let btn = $(this);
        let rolename = btn.data('rolename');

        //confirm
        let confirmation = confirm("Are you sure you want to remove " + rolename + " role from this user?");
        if (!confirmation) {
            return;
        }

        let userId = btn.data('userid');
        let roleId = btn.data('roleid');

        let data = {
            userId: userId,
            roleId: roleId,
        };

        ajaxDataRequest("/api/security/role/remove-role-from-user", "POST", data, btn, function (err, data) {
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