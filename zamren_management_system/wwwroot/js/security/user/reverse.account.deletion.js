$(document).ready(function () {
    let datatable = $("#users-table").DataTable();

    $(document).on('click', '.reverseAccountDeletionBtn', function () {
        let btn = $(this)

        let firstName = btn.data('fname');
        let lastName = btn.data('lname');

        //confirm
        let confirmation = confirm("Are you sure you want to reverse " + firstName + " " + lastName + "'s account deletion? This account will no longer be scheduled for deletion.");
        if (!confirmation) {
            return;
        }

        let userId = btn.data('userid');
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/security/user/reverse-account-deletion', 'POST', data, btn, function (err, data) {
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
