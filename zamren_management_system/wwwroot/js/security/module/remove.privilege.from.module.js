$(document).ready(function () {
    let url = new URL(window.location.href);
    let id = url.pathname.split('/').pop();

    let datatable = $("#module-privileges-table2").DataTable();

    // Handle delete button click event
    $('#module-privileges-table2 tbody').on('click', '.removePrivilegeFromModuleBtn', function () {

        let btn = $(this);
        let name = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to remove " + name + " from this module?");
        if (!confirmation) {
            return;
        }

        let privilegeId = btn.data('privilegeid');
        let data = {
            privilegeId: privilegeId,
            moduleId: id,
        };

        ajaxDataRequest("/api/security/module/remove-privilege-from-module", "POST", data, btn, function (err, data) {
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