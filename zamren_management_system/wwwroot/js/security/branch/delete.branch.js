$(document).ready(function () {

    // Handle delete button click event
    $(document).on('click', '.deleteBranchBtn', function () {

        let btn = $(this);
        let branchName = btn.data('name');

        //confirm
        let confirmation = confirm("Are you sure you want to delete this branch: " + branchName + "?");
        if (!confirmation) {
            return;
        }

        let branchId = btn.data('branchid');

        let data = {
            id: branchId,
        };

        ajaxDataRequest("/api/security/branch/delete", "POST", data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                branchesDatatable.ajax.reload();
            }
        });

    });

});