$(document).ready(function () {
    let datatable = $("#users-table").DataTable();

    $(document).on('click', '.lockUserAccountBtn', function () {
        let btn = $(this)

        let firstName = btn.data('fname');
        let lastName = btn.data('lname');

        //confirm
        let confirmation = confirm("Are you sure you want to lock " + firstName + " " + lastName + "'s account? This user will no longer be able to log into their account until you unlock their account.");
        if (!confirmation) {
            return;
        }

        let userId = btn.data('userid');
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/security/user/lock-account', 'POST', data, btn, function (err, data) {
            // console.log(err, data);
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
