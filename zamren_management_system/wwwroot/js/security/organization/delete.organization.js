$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteOrganizationBtn', function () {

        let btn = $(this);
        let organizationName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this organization: " + organizationName + "?");
        if (!confirmation) {
            return;
        }

        let organizationId = btn.data('organizationid');

        let data = {
            id: organizationId,
        };

        ajaxDataRequest("/api/security/organization/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                organizationsDatatable.ajax.reload();
            }
        });

    });

});