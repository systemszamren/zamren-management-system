$(document).ready(function () {
    let datatable = $("#users-table").DataTable();

    $(document).on('click', '.scheduleAccountDeletionBtn', function () {
        let btn = $(this)

        let firstName = btn.data('fname');
        let lastName = btn.data('lname');

        //confirm
        let confirmation = confirm("Are you sure you want to schedule " + firstName + " " + lastName + "'s account for deletion? This user will no longer be able to recover their account after the scheduled deletion date.");
        if (!confirmation) {
            return;
        }

        let userId = btn.data('userid');
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/security/user/schedule-account-deletion', 'POST', data, btn, function (err, data) {
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
