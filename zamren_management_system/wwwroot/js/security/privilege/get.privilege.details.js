$(document).ready(function () {

    window.getPrivilegeData = function () {
        //get Id from url
        let url = new URL(window.location.href);
        let id = url.pathname.split('/').pop();
        let data = {
            id: id,
        };

        ajaxDataRequest('/api/security/privilege/get-privilege-data', 'POST', data, null, function (err, data) {
            // console.log(err, data);
            if (err) {
                // Handle error
                console.error('An error occurred:', err);
            } else {
                // Handle success
                $('input[name="name"]').val(data.privilege.name);
                $('input[name="id"]').val(data.privilege.id);
                $('textarea[name="description"]').val(data.privilege.description);
            }
        });
    };

    // Call the method on page load
    getPrivilegeData();
});
