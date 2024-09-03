$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteProcessBtn', function () {

        let btn = $(this);
        let processName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this process: " + processName + "?");
        if (!confirmation) {
            return;
        }

        let processId = btn.data('processid');

        let data = {
            id: processId,
        };

        ajaxDataRequest("/api/workflow/process/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                processesDatatable.ajax.reload();
            }
        });

    });

});