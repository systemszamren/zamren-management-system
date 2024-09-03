$(document).ready(function () {
    // let datatable = $("#two-factor-authentication-table").DataTable();

    $(document).on('click', '.disable2faBtn', function () {
        let btn = $(this)

        let firstName = btn.data('fname');
        let lastName = btn.data('lname');

        //confirm
        let confirmation = confirm("Are you sure you want to disable 2FA for " + firstName + " " + lastName + "'s account? This user will no longer be required to use 2FA to log into their account.");
        if (!confirmation) {
            return;
        }

        let url = new URL(window.location.href);
        let userId = url.pathname.split('/').pop();
        let data = {
            userId: userId,
        };

        ajaxDataRequest('/api/security/user/disable-2fa', 'POST', data, btn, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
                window.twoFADatatable.ajax.reload();
            }
        });
    });
});
