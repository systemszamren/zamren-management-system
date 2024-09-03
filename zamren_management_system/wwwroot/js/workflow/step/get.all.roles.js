$(document).ready(function () {

    ajaxDataRequest('/api/workflow/step/get-roles', 'POST', null, null, function (err, data) {
        // console.log(err, data);
        if (err) {
            // Handle error
            console.error('An error occurred:', err);
        } else {
            // Handle success
            populateSelectElement(data.roles, 'roleId', '- Select Role -', null, 'id', 'name');
        }
    });
});