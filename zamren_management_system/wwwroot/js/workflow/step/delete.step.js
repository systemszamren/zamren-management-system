$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteStepBtn', function () {

        let btn = $(this);
        let stepName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this step: " + stepName + "?");
        if (!confirmation) {
            return;
        }

        let stepId = btn.data('stepid');

        let data = {
            id: stepId,
        };

        ajaxDataRequest("/api/workflow/step/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                stepsDatatable.ajax.reload();
            }
        });

    });

});