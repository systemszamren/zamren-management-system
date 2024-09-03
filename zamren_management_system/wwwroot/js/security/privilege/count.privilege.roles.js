$(document).ready(function () {
    $(document).on('click', '.countPrivilegeRolesBtn', function () {
        let btn = $(this)

        let id = $(this).val();
        let data = {
            privilegeId: id,
        };

        ajaxDataRequest('/api/security/privilege/count-roles-by-privilege-id', 'POST', data, btn, function (err, data) {
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                // console.log('Received data:', data);
                btn.text(data.totalCount);
            }
        });
    });
});
