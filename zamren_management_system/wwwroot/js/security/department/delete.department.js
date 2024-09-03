$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteDepartmentBtn', function () {

        let btn = $(this);
        let departmentName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this department: " + departmentName + "?");
        if (!confirmation) {
            return;
        }

        let departmentId = btn.data('departmentid');

        let data = {
            id: departmentId,
        };

        ajaxDataRequest("/api/security/department/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                departmentsDatatable.ajax.reload();
            }
        });

    });

});