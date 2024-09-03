$(document).ready(function () {
    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    let datatable = $("#role-users-table2").DataTable();

    // Handle delete button click event
    $('#role-users-table2 tbody').on('click', '.removeUserFromRoleBtn', function () {

        let btn = $(this);
        let firstName = btn.data('fname');
        let lastName = btn.data('lname');

        //confirm
        let confirmation = confirm("Are you sure you want to remove " + firstName + " " + lastName + " from this role?");
        if (!confirmation) {
            return;
        }

        let userId = btn.data('userid');
        let data = {
            userId: userId,
            roleId: id,
        };

        ajaxDataRequest("/api/security/role/remove-user-from-role", "POST", data, btn, function (err, data) {
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