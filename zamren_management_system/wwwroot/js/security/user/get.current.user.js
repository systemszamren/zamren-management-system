$(document).ready(function () {

    ajaxDataRequest('/api/security/user/get-current-user', 'POST', data, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            $('.currentUserFullName').html(data.user.firstName + ' ' + data.user.lastName);
            $('input[name="currentUserFullName"]').val(data.user.firstName + ' ' + data.user.lastName);

            $('.currentUserFullNameAndEmail').html(data.user.firstName + ' ' + data.user.lastName + ' (' + data.user.email + ')');
            $('input[name="currentUserFullNameAndEmail"]').val(data.user.firstName + ' ' + data.user.lastName + ' (' + data.user.email + ')');

            $('input[name="currentUserFirstName"]').val(data.user.firstName);
            $('.currentUserFirstName').html(data.user.firstName);

            $('input[name="currentUserLastName"]').val(data.user.lastName);
            $('.currentUserLastName').html(data.user.lastName);

            $('input[name="currentUserEmail"]').val(data.user.email);
            $('.currentUserEmail').html(data.user.email);

            $('input[name="currentUserId"]').val(data.user.id);
            $('.currentUserId').html(data.user.id);
        }
    });

});
