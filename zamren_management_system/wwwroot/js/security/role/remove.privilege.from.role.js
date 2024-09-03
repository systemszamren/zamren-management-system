$(document).ready(function () {
    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    let datatable = $("#role-privileges-table2").DataTable();

    // Handle delete button click event
    $('#role-privileges-table2 tbody').on('click', '.removePrivilegeFromRoleBtn', function () {

        let btn = $(this);
        let name = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to remove " + name + " from this role?");
        if (!confirmation) {
            return;
        }

        let privilegeId = btn.data('privilegeid');
        let data = {
            privilegeId: privilegeId,
            roleId: id,
        };

        ajaxDataRequest("/api/security/role/remove-privilege-from-role", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                datatable.ajax.reload();
            }
        });

    });

});