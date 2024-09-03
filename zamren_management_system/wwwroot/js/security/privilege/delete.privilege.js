$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deletePrivilegeBtn', function () {

        let btn = $(this);
        let privilegeName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this privilege: " + privilegeName + "?");
        if (!confirmation) {
            return;
        }

        let privilegeId = btn.data('privilegeid');

        let data = {
            id: privilegeId,
        };

        ajaxDataRequest("/api/security/privilege/delete-privilege", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                privilegesDatatable.ajax.reload();
            }
        });

    });

});