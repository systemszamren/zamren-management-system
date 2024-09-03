$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteOfficeBtn', function () {

        let btn = $(this);
        let officeName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this office: " + officeName + "?");
        if (!confirmation) {
            return;
        }

        let officeId = btn.data('officeid');

        let data = {
            id: officeId,
        };

        ajaxDataRequest("/api/security/office/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                officesDatatable.ajax.reload();
            }
        });

    });

});