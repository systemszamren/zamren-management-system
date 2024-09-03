$(document).ready(function () {

    // Handle reassign task button click event
    $(document).on('click', '.reassignTaskBtn', function () {

        let btn = $(this);
        let reference = btn.data('reference');
        let fullname = btn.data('fullname');

        //confirm
        let confirmation = confirm("Are you sure you want to reassign this task: " + reference + " to " + fullname + "?");
        if (!confirmation) {
            return;
        }

        let data = {
            reference: reference,
        };

        ajaxDataRequest("/api/workflow/task/reassign", "POST", data, btn, function (err, data) {
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