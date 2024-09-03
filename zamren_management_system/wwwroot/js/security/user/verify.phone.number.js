$(document).ready(function () {

    $(document).on('click', '.verifyPhoneNumberBtn', function () {
        let btn = $(this)

        let userId = btn.data('userid');
        let data = {
            userId: userId
        };

        //confirm
        if (!confirm("Are you sure you want to verify this phone number?")) {
            return;
        }

        ajaxDataRequest('/api/security/user/verify-phone-number', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                alert(err.message);
            } else {
                // Handle success
                alert(data.message);
            }
        });
    });
    
    //verifyAlternativePhoneNumberBtn
    $(document).on('click', '.verifyAlternativePhoneNumberBtn', function () {
        let btn = $(this)

        let userDetailId = btn.data('userdetailid');
        let data = {
            userDetailId: userDetailId
        };

        //confirm
        if (!confirm("Are you sure you want to verify this phone number?")) {
            return;
        }

        ajaxDataRequest('/api/security/user/verify-alternative-phone-number', 'POST', data, btn, function (err, data) {
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