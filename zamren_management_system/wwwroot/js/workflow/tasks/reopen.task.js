$(document).ready(function () {

    // Handle reopen task button click event
    $(document).on('click', '.reopenTaskBtn', function () {

        let btn = $(this);
        let reference = btn.data('reference');

        //confirm
        let confirmation = confirm("Are you sure you want to reopen this task: " + reference + "?");
        if (!confirmation) {
            return;
        }

        let data = {
            reference: reference,
        };

        ajaxDataRequest("/api/workflow/task/reopen", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
            }
        });

    });

});