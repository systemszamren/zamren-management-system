$(document).ready(function () {
    let datatable = $("#users-table").DataTable();

    $(document).on('click', '.unlockUserAccountBtn', function () {
        let btn = $(this)

        let firstName = btn.data('fname');
        let lastName = btn.data('lname');

        //confirm
        let confirmation = confirm("Are you sure you want to unlock " + firstName + " " + lastName + "'s account? This user will be able to log into their account.");
        if (!confirmation) {
            return;
        }

        let userId = btn.data('userid');
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/security/user/unlock-account', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                datatable.ajax.reload();
                getUserData();
            }
        });
    });
});
