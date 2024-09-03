$(document).ready(function () {
    $(document).on('click', '.countOfficeUsersBtn', function () {
        let btn = $(this)

        let id = $(this).val();
        let data = {
            officeId: id,
        };

        ajaxDataRequest('/api/security/office/count-users-by-office-id', 'POST', data, btn, function (err, data) {
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
