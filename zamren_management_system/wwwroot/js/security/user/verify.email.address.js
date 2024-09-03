$(document).ready(function () {

    $(document).on('click', '.verifyEmailAddressBtn', function () {
        let btn = $(this)

        let userId = btn.data('userid');
        let data = {
            userId: userId
        };

        //confirm
        if (!confirm("Are you sure you want to send a verification link to this email address?")) {
            return;
        }

        ajaxDataRequest('/api/security/user/verify-email-address', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
            }
        });
    });

    $(document).on('click', '.verifyNextOfKinEmailAddressBtn', function () {
        let btn = $(this)

        let userDetailId = btn.data('userdetailid');
        let data = {
            userDetailId: userDetailId
        };

        //confirm
        if (!confirm("Are you sure you want to send a verification link to this email address?")) {
            return;
        }

        ajaxDataRequest('/api/security/user/verify-next-of-kin-email-address', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
            }
        });
    });

    $(document).on('click', '.verifyAlternativeEmailAddressBtn', function () {
        let btn = $(this)

        let userDetailId = btn.data('userdetailid');
        let data = {
            userDetailId: userDetailId
        };

        //confirm
        if (!confirm("Are you sure you want to send a verification link to this email address?")) {
            return;
        }

        ajaxDataRequest('/api/security/user/verify-alternative-email-address', 'POST', data, btn, function (err, data) {
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
