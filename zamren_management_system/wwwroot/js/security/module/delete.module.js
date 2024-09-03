$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteModuleBtn', function () {

        let btn = $(this);
        let moduleName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this module: " + moduleName + "?");
        if (!confirmation) {
            return;
        }

        let moduleId = btn.data('moduleid');

        let data = {
            id: moduleId,
        };

        ajaxDataRequest("/api/security/module/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                modulesDatatable.ajax.reload();
            }
        });

    });

});